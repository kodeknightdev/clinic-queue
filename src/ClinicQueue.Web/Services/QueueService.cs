using ClinicQueue.Web.Data;
using ClinicQueue.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicQueue.Web.Services
{
    public class QueueService : IQueueService
    {
        private static readonly Dictionary<QueueStatus, QueueStatus[]> AllowedTransitions =
            new Dictionary<QueueStatus, QueueStatus[]>
            {
                [QueueStatus.Waiting] = new[] { QueueStatus.Called, QueueStatus.Cancelled },
                [QueueStatus.Called] = new[] { QueueStatus.InProgress, QueueStatus.NoShow, QueueStatus.Waiting },
                [QueueStatus.InProgress] = new[] { QueueStatus.Completed },
            };

        private readonly ClinicDbContext _db;
        private readonly IClock _clock;

        public QueueService(ClinicDbContext db, IClock clock)
        {
            _db = db;
            _clock = clock;
        }

        public async Task<QueueEntry> CheckInAsync(CheckInRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var now = _clock.Now;
            var patient = new Patient
            {
                FullName = request.FullName.Trim(),
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                CreatedAt = now
            };

            var entry = await CreateEntryAsync(patient, request, now);
            _db.QueueEntries.Add(entry);
            await _db.SaveChangesAsync();

            return entry;
        }

        public Task<QueueEntry> FindByLookupCodeAsync(string code)
        {
            var normalized = LookupCode.Normalize(code);
            return _db.QueueEntries
                .Include(e => e.Patient)
                .SingleOrDefaultAsync(e => e.LookupCode == normalized);
        }

        public async Task<int?> GetPositionAsync(QueueEntry entry)
        {
            if (entry.Status != QueueStatus.Waiting)
            {
                return null;
            }

            // Priority entries go ahead of everyone else; within the same group it's ticket order.
            var ahead = await _db.QueueEntries.CountAsync(e =>
                e.QueueDate == entry.QueueDate &&
                e.Status == QueueStatus.Waiting &&
                e.Id != entry.Id &&
                ((e.IsPriority && !entry.IsPriority) ||
                 (e.IsPriority == entry.IsPriority && e.TicketNumber < entry.TicketNumber)));

            return ahead + 1;
        }

        public async Task<IReadOnlyList<QueueEntry>> GetTodayAsync()
        {
            var today = _clock.Now.Date;
            return await _db.QueueEntries
                .Include(e => e.Patient)
                .Where(e => e.QueueDate == today)
                .OrderByDescending(e => e.IsPriority)
                .ThenBy(e => e.TicketNumber)
                .ToListAsync();
        }

        public async Task<StatusChangeResult> ChangeStatusAsync(int entryId, QueueStatus newStatus)
        {
            var entry = await _db.QueueEntries.FindAsync(entryId);
            if (entry == null)
            {
                return StatusChangeResult.NotFound;
            }

            if (!CanTransition(entry.Status, newStatus))
            {
                return StatusChangeResult.InvalidTransition;
            }

            var now = _clock.Now;
            switch (newStatus)
            {
                case QueueStatus.Waiting:
                    // back in the queue, keeps the original ticket number
                    entry.CalledAt = null;
                    break;
                case QueueStatus.Called:
                    entry.CalledAt = now;
                    break;
                case QueueStatus.InProgress:
                    entry.StartedAt = now;
                    break;
                case QueueStatus.Completed:
                case QueueStatus.NoShow:
                case QueueStatus.Cancelled:
                    entry.CompletedAt = now;
                    break;
            }

            entry.Status = newStatus;
            await _db.SaveChangesAsync();

            return StatusChangeResult.Updated;
        }

        public static bool CanTransition(QueueStatus from, QueueStatus to)
        {
            return AllowedTransitions.TryGetValue(from, out var allowed) && Array.IndexOf(allowed, to) >= 0;
        }

        private async Task<QueueEntry> CreateEntryAsync(Patient patient, CheckInRequest request, DateTime now)
        {
            var today = now.Date;
            var lastTicket = await _db.QueueEntries
                .Where(e => e.QueueDate == today)
                .MaxAsync(e => (int?)e.TicketNumber);

            return new QueueEntry
            {
                Patient = patient,
                QueueDate = today,
                TicketNumber = (lastTicket ?? 0) + 1,
                LookupCode = LookupCode.Generate(),
                Reason = request.Reason?.Trim(),
                IsPriority = request.IsPriority,
                Status = QueueStatus.Waiting,
                CheckedInAt = now
            };
        }
    }
}

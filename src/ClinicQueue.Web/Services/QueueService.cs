using ClinicQueue.Web.Data;
using ClinicQueue.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClinicQueue.Web.Services
{
    public class QueueService : IQueueService
    {
        // Used for estimates until the first consult of the day has finished.
        private const double DefaultConsultMinutes = 12;

        private const int MaxCheckInAttempts = 3;

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
            var patient = await FindOrCreatePatientAsync(request, now);

            for (var attempt = 1; ; attempt++)
            {
                var entry = await CreateEntryAsync(patient, request, now);
                _db.QueueEntries.Add(entry);

                try
                {
                    await _db.SaveChangesAsync();
                    return entry;
                }
                catch (DbUpdateException) when (attempt < MaxCheckInAttempts)
                {
                    // Two desks checked someone in at the same moment and got the same ticket
                    // number (unique index). Drop this entry and try again with the next number.
                    _db.Entry(entry).State = EntityState.Detached;
                }
            }
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

        public async Task<QueueSnapshot> GetSnapshotAsync()
        {
            var entries = await GetTodayAsync();
            var now = _clock.Now;
            var waiting = entries.Where(e => e.Status == QueueStatus.Waiting).ToList();

            return new QueueSnapshot
            {
                Token = ComputeToken(entries),
                WaitingCount = waiting.Count,
                CalledCount = entries.Count(e => e.Status == QueueStatus.Called),
                InProgressCount = entries.Count(e => e.Status == QueueStatus.InProgress),
                CompletedCount = entries.Count(e => e.Status == QueueStatus.Completed),
                Waiting = waiting.Select((e, i) => new QueueSnapshotItem
                {
                    Position = i + 1,
                    Ticket = e.TicketNumber,
                    Name = e.Patient.FullName,
                    IsPriority = e.IsPriority,
                    WaitedMinutes = (int)(now - e.CheckedInAt).TotalMinutes
                }).ToList()
            };
        }

        public async Task<int?> EstimateWaitMinutesAsync(int? position)
        {
            if (position == null)
            {
                return null;
            }

            var average = await AverageConsultMinutesAsync() ?? DefaultConsultMinutes;
            return (int)Math.Ceiling(average * (position.Value - 1));
        }

        public static bool CanTransition(QueueStatus from, QueueStatus to)
        {
            return AllowedTransitions.TryGetValue(from, out var allowed) && Array.IndexOf(allowed, to) >= 0;
        }

        /// <summary>
        /// Short fingerprint of the queue (ids + statuses). The dashboard compares the value it
        /// was rendered with against the live one to tell whether its tables are out of date.
        /// </summary>
        public static string ComputeToken(IEnumerable<QueueEntry> entries)
        {
            var raw = string.Join(";", entries.OrderBy(e => e.Id).Select(e => e.Id + ":" + (int)e.Status));
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(hash, 0, 6).Replace("-", "").ToLowerInvariant();
        }

        private async Task<Patient> FindOrCreatePatientAsync(CheckInRequest request, DateTime now)
        {
            var name = request.FullName.Trim();
            var phone = PhoneNumber.Normalize(request.Phone);

            // Returning patients: match on phone + name so we don't pile up duplicates.
            if (phone != null)
            {
                var existing = await _db.Patients
                    .FirstOrDefaultAsync(p => p.Phone == phone && p.FullName == name);
                if (existing != null)
                {
                    return existing;
                }
            }

            return new Patient
            {
                FullName = name,
                Phone = phone,
                DateOfBirth = request.DateOfBirth,
                CreatedAt = now
            };
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

        private async Task<double?> AverageConsultMinutesAsync()
        {
            var today = _clock.Now.Date;
            var consults = await _db.QueueEntries
                .Where(e => e.QueueDate == today && e.StartedAt != null && e.CompletedAt != null)
                .Select(e => new { e.StartedAt, e.CompletedAt })
                .ToListAsync();

            if (consults.Count == 0)
            {
                return null;
            }

            return consults.Average(c => (c.CompletedAt.Value - c.StartedAt.Value).TotalMinutes);
        }
    }
}

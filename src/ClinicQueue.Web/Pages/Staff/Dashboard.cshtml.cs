using ClinicQueue.Web.Data;
using ClinicQueue.Web.Models;
using ClinicQueue.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicQueue.Web.Pages.Staff
{
    public class DashboardModel : PageModel
    {
        private readonly ClinicDbContext _db;
        private readonly IClock _clock;

        public DashboardModel(ClinicDbContext db, IClock clock)
        {
            _db = db;
            _clock = clock;
        }

        public DateTime Now { get; private set; }

        public IList<QueueEntry> Waiting { get; private set; } = new List<QueueEntry>();

        public IList<QueueEntry> Called { get; private set; } = new List<QueueEntry>();

        public IList<QueueEntry> InProgress { get; private set; } = new List<QueueEntry>();

        public IList<QueueEntry> Done { get; private set; } = new List<QueueEntry>();

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        public Task<IActionResult> OnPostCallAsync(int id) =>
            UpdateStatusAsync(id, QueueStatus.Waiting, QueueStatus.Called, e => e.CalledAt = _clock.Now);

        public Task<IActionResult> OnPostStartAsync(int id) =>
            UpdateStatusAsync(id, QueueStatus.Called, QueueStatus.InProgress, e => e.StartedAt = _clock.Now);

        public Task<IActionResult> OnPostCompleteAsync(int id) =>
            UpdateStatusAsync(id, QueueStatus.InProgress, QueueStatus.Completed, e => e.CompletedAt = _clock.Now);

        public Task<IActionResult> OnPostNoShowAsync(int id) =>
            UpdateStatusAsync(id, QueueStatus.Called, QueueStatus.NoShow, e => e.CompletedAt = _clock.Now);

        private async Task<IActionResult> UpdateStatusAsync(int id, QueueStatus from, QueueStatus to, Action<QueueEntry> apply)
        {
            var entry = await _db.QueueEntries.FindAsync(id);
            if (entry == null)
            {
                return NotFound();
            }

            // Someone else may have already moved this entry on (two desks, stale page).
            if (entry.Status != from)
            {
                return RedirectToPage();
            }

            entry.Status = to;
            apply(entry);
            await _db.SaveChangesAsync();

            return RedirectToPage();
        }

        private async Task LoadAsync()
        {
            Now = _clock.Now;
            var today = Now.Date;
            var entries = await _db.QueueEntries
                .Include(e => e.Patient)
                .Where(e => e.QueueDate == today)
                .OrderByDescending(e => e.IsPriority)
                .ThenBy(e => e.TicketNumber)
                .ToListAsync();

            Waiting = entries.Where(e => e.Status == QueueStatus.Waiting).ToList();
            Called = entries.Where(e => e.Status == QueueStatus.Called).ToList();
            InProgress = entries.Where(e => e.Status == QueueStatus.InProgress).ToList();
            Done = entries.Where(e => e.Status == QueueStatus.Completed || e.Status == QueueStatus.NoShow).ToList();
        }
    }
}

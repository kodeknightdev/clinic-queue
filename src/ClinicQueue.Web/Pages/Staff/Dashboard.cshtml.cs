using ClinicQueue.Web.Data;
using ClinicQueue.Web.Models;
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

        public DashboardModel(ClinicDbContext db)
        {
            _db = db;
        }

        public IList<QueueEntry> Waiting { get; private set; } = new List<QueueEntry>();

        public IList<QueueEntry> Called { get; private set; } = new List<QueueEntry>();

        public IList<QueueEntry> InProgress { get; private set; } = new List<QueueEntry>();

        public IList<QueueEntry> Done { get; private set; } = new List<QueueEntry>();

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        public Task<IActionResult> OnPostCallAsync(int id) =>
            UpdateStatusAsync(id, QueueStatus.Waiting, QueueStatus.Called, e => e.CalledAt = DateTime.UtcNow);

        public Task<IActionResult> OnPostStartAsync(int id) =>
            UpdateStatusAsync(id, QueueStatus.Called, QueueStatus.InProgress, e => e.StartedAt = DateTime.UtcNow);

        public Task<IActionResult> OnPostCompleteAsync(int id) =>
            UpdateStatusAsync(id, QueueStatus.InProgress, QueueStatus.Completed, e => e.CompletedAt = DateTime.UtcNow);

        public Task<IActionResult> OnPostNoShowAsync(int id) =>
            UpdateStatusAsync(id, QueueStatus.Called, QueueStatus.NoShow, e => e.CompletedAt = DateTime.UtcNow);

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
            var today = DateTime.UtcNow.Date;
            var entries = await _db.QueueEntries
                .Include(e => e.Patient)
                .Where(e => e.QueueDate == today)
                .OrderBy(e => e.TicketNumber)
                .ToListAsync();

            Waiting = entries.Where(e => e.Status == QueueStatus.Waiting).ToList();
            Called = entries.Where(e => e.Status == QueueStatus.Called).ToList();
            InProgress = entries.Where(e => e.Status == QueueStatus.InProgress).ToList();
            Done = entries.Where(e => e.Status == QueueStatus.Completed || e.Status == QueueStatus.NoShow).ToList();
        }
    }
}

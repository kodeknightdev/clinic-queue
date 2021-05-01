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

        public IList<QueueEntry> Done { get; private set; } = new List<QueueEntry>();

        public async Task OnGetAsync()
        {
            await LoadAsync();
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
            Done = entries.Where(e => e.Status == QueueStatus.Completed || e.Status == QueueStatus.NoShow).ToList();
        }
    }
}

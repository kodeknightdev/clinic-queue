using ClinicQueue.Web.Data;
using ClinicQueue.Web.Models;
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

        public IList<QueueEntry> Waiting { get; private set; }

        public async Task OnGetAsync()
        {
            var today = DateTime.UtcNow.Date;
            Waiting = await _db.QueueEntries
                .Include(e => e.Patient)
                .Where(e => e.QueueDate == today && e.Status == QueueStatus.Waiting)
                .OrderBy(e => e.TicketNumber)
                .ToListAsync();
        }
    }
}

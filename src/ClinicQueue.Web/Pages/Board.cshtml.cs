using ClinicQueue.Web.Data;
using ClinicQueue.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicQueue.Web.Pages
{
    /// <summary>Waiting room display (meant for a TV).</summary>
    public class BoardModel : PageModel
    {
        private readonly ClinicDbContext _db;

        public BoardModel(ClinicDbContext db)
        {
            _db = db;
        }

        public IList<QueueEntry> NowServing { get; private set; }

        public IList<QueueEntry> UpNext { get; private set; }

        public async Task OnGetAsync()
        {
            NowServing = await _db.QueueEntries
                .Include(e => e.Patient)
                .Where(e => e.Status == QueueStatus.Called)
                .OrderByDescending(e => e.CalledAt)
                .Take(4)
                .ToListAsync();

            UpNext = await _db.QueueEntries
                .Include(e => e.Patient)
                .Where(e => e.Status == QueueStatus.Waiting)
                .OrderByDescending(e => e.IsPriority)
                .ThenBy(e => e.TicketNumber)
                .Take(6)
                .ToListAsync();
        }
    }
}

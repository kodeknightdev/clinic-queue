using ClinicQueue.Web.Data;
using ClinicQueue.Web.Models;
using ClinicQueue.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ClinicQueue.Web.Pages
{
    public class StatusModel : PageModel
    {
        private readonly ClinicDbContext _db;

        public StatusModel(ClinicDbContext db)
        {
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public string Code { get; set; }

        public QueueEntry Entry { get; private set; }

        public int? Position { get; private set; }

        public bool NotFoundCode { get; private set; }

        public async Task OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                return;
            }

            var code = LookupCode.Normalize(Code);
            Entry = await _db.QueueEntries
                .Include(e => e.Patient)
                .SingleOrDefaultAsync(e => e.LookupCode == code);

            if (Entry == null)
            {
                NotFoundCode = true;
                return;
            }

            if (Entry.Status == QueueStatus.Waiting)
            {
                var ahead = await _db.QueueEntries.CountAsync(e =>
                    e.QueueDate == Entry.QueueDate &&
                    e.Status == QueueStatus.Waiting &&
                    e.Id != Entry.Id &&
                    ((e.IsPriority && !Entry.IsPriority) ||
                     (e.IsPriority == Entry.IsPriority && e.TicketNumber < Entry.TicketNumber)));

                Position = ahead + 1;
            }
        }
    }
}

using ClinicQueue.Web.Data;
using ClinicQueue.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ClinicQueue.Web.Pages
{
    public class CheckedInModel : PageModel
    {
        private readonly ClinicDbContext _db;

        public CheckedInModel(ClinicDbContext db)
        {
            _db = db;
        }

        public QueueEntry Entry { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Entry = await _db.QueueEntries
                .Include(e => e.Patient)
                .SingleOrDefaultAsync(e => e.Id == id);

            if (Entry == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}

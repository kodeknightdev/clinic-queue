using ClinicQueue.Web.Data;
using ClinicQueue.Web.Models;
using ClinicQueue.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicQueue.Web.Pages
{
    public class CheckInModel : PageModel
    {
        private readonly ClinicDbContext _db;

        public CheckInModel(ClinicDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public CheckInInput Input { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var now = DateTime.UtcNow;
            var today = now.Date;
            var lastTicket = await _db.QueueEntries
                .Where(e => e.QueueDate == today)
                .MaxAsync(e => (int?)e.TicketNumber);

            var patient = new Patient
            {
                FullName = Input.FullName,
                Phone = Input.Phone,
                DateOfBirth = Input.DateOfBirth,
                CreatedAt = now
            };

            var entry = new QueueEntry
            {
                Patient = patient,
                QueueDate = today,
                TicketNumber = (lastTicket ?? 0) + 1,
                LookupCode = LookupCode.Generate(),
                Status = QueueStatus.Waiting,
                Reason = Input.Reason,
                CheckedInAt = now
            };

            _db.QueueEntries.Add(entry);
            await _db.SaveChangesAsync();

            return RedirectToPage("/CheckedIn", new { id = entry.Id });
        }

        public class CheckInInput
        {
            [Required]
            [StringLength(100)]
            [Display(Name = "Full name")]
            public string FullName { get; set; }

            [Phone]
            [StringLength(20)]
            public string Phone { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "Date of birth")]
            public DateTime? DateOfBirth { get; set; }

            [StringLength(200)]
            [Display(Name = "Reason for visit")]
            public string Reason { get; set; }
        }
    }
}

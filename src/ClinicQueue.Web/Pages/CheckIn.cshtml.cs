using ClinicQueue.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ClinicQueue.Web.Pages
{
    public class CheckInModel : PageModel
    {
        private readonly IQueueService _queue;

        public CheckInModel(IQueueService queue)
        {
            _queue = queue;
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

            var entry = await _queue.CheckInAsync(new CheckInRequest
            {
                FullName = Input.FullName,
                Phone = Input.Phone,
                DateOfBirth = Input.DateOfBirth,
                Reason = Input.Reason,
                IsPriority = Input.IsPriority
            });

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

            [Display(Name = "Urgent (front desk use)")]
            public bool IsPriority { get; set; }
        }
    }
}

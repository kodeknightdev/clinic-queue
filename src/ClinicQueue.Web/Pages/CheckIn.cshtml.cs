using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicQueue.Web.Pages
{
    public class CheckInModel : PageModel
    {
        [BindProperty]
        public CheckInInput Input { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: save
            return RedirectToPage("/Index");
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

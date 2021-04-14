using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

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
            // TODO: save
            return RedirectToPage("/Index");
        }

        public class CheckInInput
        {
            public string FullName { get; set; }

            public string Phone { get; set; }

            public DateTime? DateOfBirth { get; set; }

            public string Reason { get; set; }
        }
    }
}

using ClinicQueue.Web.Models;
using ClinicQueue.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace ClinicQueue.Web.Pages
{
    public class StatusModel : PageModel
    {
        private readonly IQueueService _queue;

        public StatusModel(IQueueService queue)
        {
            _queue = queue;
        }

        [BindProperty(SupportsGet = true)]
        public string Code { get; set; }

        /// <summary>Set when we've just redirected here from the check-in form.</summary>
        [BindProperty(SupportsGet = true)]
        public bool CheckedIn { get; set; }

        public QueueEntry Entry { get; private set; }

        public int? Position { get; private set; }

        public bool NotFoundCode { get; private set; }

        public async Task OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                return;
            }

            Entry = await _queue.FindByLookupCodeAsync(Code);
            if (Entry == null)
            {
                NotFoundCode = true;
                return;
            }

            Position = await _queue.GetPositionAsync(Entry);
        }

        /// <summary>JSON for status-poll.js.</summary>
        public async Task<IActionResult> OnGetPositionAsync()
        {
            var entry = string.IsNullOrWhiteSpace(Code) ? null : await _queue.FindByLookupCodeAsync(Code);
            if (entry == null)
            {
                return NotFound();
            }

            var position = await _queue.GetPositionAsync(entry);
            return new JsonResult(new
            {
                status = entry.Status.ToString(),
                statusLabel = entry.Status.Label(),
                position,
                isFinal = entry.Status.IsFinal()
            });
        }
    }
}

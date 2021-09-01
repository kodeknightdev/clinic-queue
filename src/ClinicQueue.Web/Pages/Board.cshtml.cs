using ClinicQueue.Web.Models;
using ClinicQueue.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicQueue.Web.Pages
{
    /// <summary>Waiting room display (meant for a TV).</summary>
    public class BoardModel : PageModel
    {
        private readonly IQueueService _queue;

        public BoardModel(IQueueService queue)
        {
            _queue = queue;
        }

        public IList<QueueEntry> NowServing { get; private set; }

        public IList<QueueEntry> UpNext { get; private set; }

        public async Task OnGetAsync()
        {
            // Only today's entries; already ordered priority first, then by ticket.
            var today = await _queue.GetTodayAsync();

            NowServing = today
                .Where(e => e.Status == QueueStatus.Called)
                .OrderByDescending(e => e.CalledAt)
                .Take(4)
                .ToList();

            UpNext = today
                .Where(e => e.Status == QueueStatus.Waiting)
                .Take(6)
                .ToList();
        }
    }
}

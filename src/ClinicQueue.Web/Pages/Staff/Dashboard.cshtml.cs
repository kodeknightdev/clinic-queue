using ClinicQueue.Web.Models;
using ClinicQueue.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicQueue.Web.Pages.Staff
{
    public class DashboardModel : PageModel
    {
        private readonly IQueueService _queue;
        private readonly IClock _clock;

        public DashboardModel(IQueueService queue, IClock clock)
        {
            _queue = queue;
            _clock = clock;
        }

        public DateTime Now { get; private set; }

        public IList<QueueEntry> Waiting { get; private set; } = new List<QueueEntry>();

        public IList<QueueEntry> Called { get; private set; } = new List<QueueEntry>();

        public IList<QueueEntry> InProgress { get; private set; } = new List<QueueEntry>();

        public IList<QueueEntry> Done { get; private set; } = new List<QueueEntry>();

        /// <summary>Fingerprint of the queue as rendered; the live panel compares against it.</summary>
        public string QueueToken { get; private set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        public async Task<IActionResult> OnGetSnapshotAsync()
        {
            var snapshot = await _queue.GetSnapshotAsync();
            return new JsonResult(snapshot);
        }

        public Task<IActionResult> OnPostCallAsync(int id) => ChangeStatusAsync(id, QueueStatus.Called);

        public Task<IActionResult> OnPostStartAsync(int id) => ChangeStatusAsync(id, QueueStatus.InProgress);

        public Task<IActionResult> OnPostCompleteAsync(int id) => ChangeStatusAsync(id, QueueStatus.Completed);

        public Task<IActionResult> OnPostNoShowAsync(int id) => ChangeStatusAsync(id, QueueStatus.NoShow);

        public Task<IActionResult> OnPostRequeueAsync(int id) => ChangeStatusAsync(id, QueueStatus.Waiting);

        public Task<IActionResult> OnPostCancelAsync(int id) => ChangeStatusAsync(id, QueueStatus.Cancelled);

        private async Task<IActionResult> ChangeStatusAsync(int id, QueueStatus status)
        {
            var result = await _queue.ChangeStatusAsync(id, status);
            StatusMessage = Describe(result);
            return RedirectToPage();
        }

        private static string Describe(StatusChangeResult result)
        {
            switch (result)
            {
                case StatusChangeResult.NotFound:
                    return "That queue entry no longer exists.";
                case StatusChangeResult.InvalidTransition:
                    return "That entry had already moved on, so nothing was changed.";
                default:
                    return null;
            }
        }

        private async Task LoadAsync()
        {
            Now = _clock.Now;
            var entries = await _queue.GetTodayAsync();

            Waiting = entries.Where(e => e.Status == QueueStatus.Waiting).ToList();
            Called = entries.Where(e => e.Status == QueueStatus.Called).ToList();
            InProgress = entries.Where(e => e.Status == QueueStatus.InProgress).ToList();
            Done = entries.Where(e => e.Status.IsFinal()).ToList();
            QueueToken = QueueService.ComputeToken(entries);
        }
    }
}

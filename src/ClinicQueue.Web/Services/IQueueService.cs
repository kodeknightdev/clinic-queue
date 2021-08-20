using ClinicQueue.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicQueue.Web.Services
{
    public interface IQueueService
    {
        Task<QueueEntry> CheckInAsync(CheckInRequest request);

        Task<QueueEntry> FindByLookupCodeAsync(string code);

        /// <summary>1-based place in the waiting line, or null if the entry isn't waiting.</summary>
        Task<int?> GetPositionAsync(QueueEntry entry);

        /// <summary>Today's entries, priority first then by ticket number.</summary>
        Task<IReadOnlyList<QueueEntry>> GetTodayAsync();

        Task<StatusChangeResult> ChangeStatusAsync(int entryId, QueueStatus newStatus);

        Task<bool> UpdateNotesAsync(int entryId, string notes);

        Task<QueueSnapshot> GetSnapshotAsync();

        Task<int?> EstimateWaitMinutesAsync(int? position);
    }
}

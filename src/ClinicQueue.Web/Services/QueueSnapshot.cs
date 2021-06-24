using System.Collections.Generic;

namespace ClinicQueue.Web.Services
{
    /// <summary>What the dashboard's live queue panel polls for.</summary>
    public class QueueSnapshot
    {
        public string Token { get; set; }

        public int WaitingCount { get; set; }

        public int CalledCount { get; set; }

        public int InProgressCount { get; set; }

        public int CompletedCount { get; set; }

        public IList<QueueSnapshotItem> Waiting { get; set; } = new List<QueueSnapshotItem>();
    }

    public class QueueSnapshotItem
    {
        public int Position { get; set; }

        public int Ticket { get; set; }

        public string Name { get; set; }

        public bool IsPriority { get; set; }

        public int WaitedMinutes { get; set; }
    }
}

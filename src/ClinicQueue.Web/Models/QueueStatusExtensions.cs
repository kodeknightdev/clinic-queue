namespace ClinicQueue.Web.Models
{
    public static class QueueStatusExtensions
    {
        public static string Label(this QueueStatus status)
        {
            switch (status)
            {
                case QueueStatus.Waiting:
                    return "Waiting";
                case QueueStatus.Called:
                    return "Called";
                case QueueStatus.InProgress:
                    return "With clinician";
                case QueueStatus.Completed:
                    return "Done";
                case QueueStatus.NoShow:
                    return "No-show";
                case QueueStatus.Cancelled:
                    return "Cancelled";
                default:
                    return status.ToString();
            }
        }

        /// <summary>True once an entry has left the queue for good.</summary>
        public static bool IsFinal(this QueueStatus status)
        {
            return status == QueueStatus.Completed
                || status == QueueStatus.NoShow
                || status == QueueStatus.Cancelled;
        }
    }
}

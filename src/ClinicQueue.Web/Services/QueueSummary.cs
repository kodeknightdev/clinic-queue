namespace ClinicQueue.Web.Services
{
    public class QueueSummary
    {
        public int CheckedIn { get; set; }

        public int Seen { get; set; }

        public int NoShows { get; set; }

        public double? AverageConsultMinutes { get; set; }
    }
}

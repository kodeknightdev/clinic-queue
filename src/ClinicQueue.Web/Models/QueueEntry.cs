using System;

namespace ClinicQueue.Web.Models
{
    public class QueueEntry
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        public Patient Patient { get; set; }

        /// <summary>The clinic day this entry belongs to (date only).</summary>
        public DateTime QueueDate { get; set; }

        /// <summary>Sequential per day, starting at 1.</summary>
        public int TicketNumber { get; set; }

        public string LookupCode { get; set; }

        public QueueStatus Status { get; set; }

        public string Reason { get; set; }

        public DateTime CheckedInAt { get; set; }

        public DateTime? CalledAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}

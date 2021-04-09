using System;

namespace ClinicQueue.Web.Models
{
    public class QueueEntry
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        public Patient Patient { get; set; }

        public QueueStatus Status { get; set; }

        public string Reason { get; set; }

        public DateTime CheckedInAt { get; set; }

        public DateTime? CalledAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}

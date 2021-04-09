using System;
using System.Collections.Generic;

namespace ClinicQueue.Web.Models
{
    public class Patient
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Phone { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<QueueEntry> QueueEntries { get; set; } = new List<QueueEntry>();
    }
}

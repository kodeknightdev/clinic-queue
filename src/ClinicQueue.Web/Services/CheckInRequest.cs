using System;

namespace ClinicQueue.Web.Services
{
    public class CheckInRequest
    {
        public string FullName { get; set; }

        public string Phone { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Reason { get; set; }

        public bool IsPriority { get; set; }
    }
}

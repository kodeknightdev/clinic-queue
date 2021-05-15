using System;

namespace ClinicQueue.Web.Services
{
    /// <summary>
    /// Abstracts "now" so queue days line up with the clinic's local day
    /// (and so tests can control time).
    /// </summary>
    public interface IClock
    {
        DateTime Now { get; }
    }

    public class LocalClock : IClock
    {
        // The app runs on a machine in the clinic, so server local time == clinic time.
        public DateTime Now => DateTime.Now;
    }
}

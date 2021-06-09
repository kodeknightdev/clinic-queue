using ClinicQueue.Web.Data;
using ClinicQueue.Web.Services;
using Microsoft.EntityFrameworkCore;
using System;

namespace ClinicQueue.Tests
{
    internal static class TestDb
    {
        public static ClinicDbContext Create()
        {
            var options = new DbContextOptionsBuilder<ClinicDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ClinicDbContext(options);
        }
    }

    internal class FakeClock : IClock
    {
        public FakeClock(DateTime now)
        {
            Now = now;
        }

        public DateTime Now { get; set; }

        public void Advance(TimeSpan by)
        {
            Now = Now.Add(by);
        }
    }
}

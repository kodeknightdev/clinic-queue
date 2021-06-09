using ClinicQueue.Web.Models;
using ClinicQueue.Web.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ClinicQueue.Tests
{
    public class QueueServiceTests
    {
        private readonly FakeClock _clock = new FakeClock(new DateTime(2021, 6, 1, 9, 0, 0));
        private readonly QueueService _service;

        public QueueServiceTests()
        {
            _service = new QueueService(TestDb.Create(), _clock);
        }

        private Task<QueueEntry> CheckIn(string name, bool priority = false)
        {
            return _service.CheckInAsync(new CheckInRequest { FullName = name, IsPriority = priority });
        }

        [Fact]
        public async Task CheckIn_AssignsSequentialTicketNumbers()
        {
            var first = await CheckIn("Ana Silva");
            var second = await CheckIn("Ben Okafor");

            Assert.Equal(1, first.TicketNumber);
            Assert.Equal(2, second.TicketNumber);
            Assert.Equal(QueueStatus.Waiting, second.Status);
            Assert.False(string.IsNullOrEmpty(second.LookupCode));
        }
    }
}

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

        [Fact]
        public async Task TicketNumbers_StartAgainTheNextDay()
        {
            await CheckIn("Ana Silva");
            await CheckIn("Ben Okafor");

            _clock.Advance(TimeSpan.FromDays(1));
            var nextDay = await CheckIn("Cara Byrne");

            Assert.Equal(1, nextDay.TicketNumber);
        }

        [Fact]
        public async Task Position_CountsOnlyWaitingEntriesAhead()
        {
            var a = await CheckIn("Ana Silva");
            var b = await CheckIn("Ben Okafor");
            var c = await CheckIn("Cara Byrne");

            await _service.ChangeStatusAsync(a.Id, QueueStatus.Called);

            Assert.Null(await _service.GetPositionAsync(a));
            Assert.Equal(1, await _service.GetPositionAsync(b));
            Assert.Equal(2, await _service.GetPositionAsync(c));
        }

        [Fact]
        public async Task Position_PriorityEntriesGoFirst()
        {
            var a = await CheckIn("Ana Silva");
            var urgent = await CheckIn("Dev Patel", priority: true);

            Assert.Equal(1, await _service.GetPositionAsync(urgent));
            Assert.Equal(2, await _service.GetPositionAsync(a));
        }
    }
}

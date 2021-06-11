using ClinicQueue.Web.Models;
using ClinicQueue.Web.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ClinicQueue.Tests
{
    public class StatusTransitionTests
    {
        private readonly FakeClock _clock = new FakeClock(new DateTime(2021, 6, 1, 9, 0, 0));
        private readonly QueueService _service;

        public StatusTransitionTests()
        {
            _service = new QueueService(TestDb.Create(), _clock);
        }

        [Theory]
        [InlineData(QueueStatus.Waiting, QueueStatus.Called, true)]
        [InlineData(QueueStatus.Called, QueueStatus.InProgress, true)]
        [InlineData(QueueStatus.InProgress, QueueStatus.Completed, true)]
        [InlineData(QueueStatus.Called, QueueStatus.Waiting, true)]
        [InlineData(QueueStatus.Waiting, QueueStatus.Cancelled, true)]
        [InlineData(QueueStatus.Waiting, QueueStatus.Completed, false)]
        [InlineData(QueueStatus.Completed, QueueStatus.Waiting, false)]
        [InlineData(QueueStatus.NoShow, QueueStatus.Called, false)]
        public void CanTransition(QueueStatus from, QueueStatus to, bool expected)
        {
            Assert.Equal(expected, QueueService.CanTransition(from, to));
        }

        [Fact]
        public async Task Call_SetsCalledAt()
        {
            var entry = await _service.CheckInAsync(new CheckInRequest { FullName = "Ana Silva" });
            _clock.Advance(TimeSpan.FromMinutes(7));

            var result = await _service.ChangeStatusAsync(entry.Id, QueueStatus.Called);

            Assert.Equal(StatusChangeResult.Updated, result);
            Assert.Equal(_clock.Now, entry.CalledAt);
        }

        [Fact]
        public async Task CompletingAWaitingEntry_IsRejected()
        {
            var entry = await _service.CheckInAsync(new CheckInRequest { FullName = "Ana Silva" });

            var result = await _service.ChangeStatusAsync(entry.Id, QueueStatus.Completed);

            Assert.Equal(StatusChangeResult.InvalidTransition, result);
            Assert.Equal(QueueStatus.Waiting, entry.Status);
        }

        [Fact]
        public async Task Requeue_ClearsCalledAt()
        {
            var entry = await _service.CheckInAsync(new CheckInRequest { FullName = "Ana Silva" });
            await _service.ChangeStatusAsync(entry.Id, QueueStatus.Called);

            await _service.ChangeStatusAsync(entry.Id, QueueStatus.Waiting);

            Assert.Equal(QueueStatus.Waiting, entry.Status);
            Assert.Null(entry.CalledAt);
        }

        [Fact]
        public async Task UnknownEntry_ReturnsNotFound()
        {
            var result = await _service.ChangeStatusAsync(999, QueueStatus.Called);

            Assert.Equal(StatusChangeResult.NotFound, result);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToggleNet.Core.Entities;
using ToggleNet.Core.Scheduling;
using ToggleNet.Core.Storage;
using Xunit;
using Moq;

namespace ToggleNet.Core.Tests
{
    public class TimeBasedSchedulingTests
    {
        private readonly Mock<IFeatureStore> _featureStoreMock;
        private readonly FeatureFlagScheduler _scheduler;
        private readonly string _environment = "Test";

        public TimeBasedSchedulingTests()
        {
            _featureStoreMock = new Mock<IFeatureStore>();
            _scheduler = new FeatureFlagScheduler(_featureStoreMock.Object, _environment);
        }

        [Fact]
        public void IsTimeActive_WithoutTimeBasedActivation_ReturnsTrue()
        {
            // Arrange
            var flag = new FeatureFlag
            {
                Name = "test-flag",
                IsEnabled = true,
                Environment = _environment,
                UseTimeBasedActivation = false
            };

            // Act
            var result = flag.IsTimeActive();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsTimeActive_WithStartTimeInFuture_ReturnsFalse()
        {
            // Arrange
            var futureTime = DateTime.UtcNow.AddHours(1);
            var flag = new FeatureFlag
            {
                Name = "test-flag",
                IsEnabled = true,
                Environment = _environment,
                UseTimeBasedActivation = true,
                StartTime = futureTime
            };

            // Act
            var result = flag.IsTimeActive();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsTimeActive_WithStartTimeInPast_ReturnsTrue()
        {
            // Arrange
            var pastTime = DateTime.UtcNow.AddHours(-1);
            var flag = new FeatureFlag
            {
                Name = "test-flag",
                IsEnabled = true,
                Environment = _environment,
                UseTimeBasedActivation = true,
                StartTime = pastTime
            };

            // Act
            var result = flag.IsTimeActive();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsTimeActive_WithEndTimeInPast_ReturnsFalse()
        {
            // Arrange
            var pastTime = DateTime.UtcNow.AddHours(-1);
            var flag = new FeatureFlag
            {
                Name = "test-flag",
                IsEnabled = true,
                Environment = _environment,
                UseTimeBasedActivation = true,
                EndTime = pastTime
            };

            // Act
            var result = flag.IsTimeActive();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsTimeActive_WithValidTimeWindow_ReturnsTrue()
        {
            // Arrange
            var startTime = DateTime.UtcNow.AddHours(-1);
            var endTime = DateTime.UtcNow.AddHours(1);
            var flag = new FeatureFlag
            {
                Name = "test-flag",
                IsEnabled = true,
                Environment = _environment,
                UseTimeBasedActivation = true,
                StartTime = startTime,
                EndTime = endTime
            };

            // Act
            var result = flag.IsTimeActive();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsTimeActive_WithDurationExpired_ReturnsFalse()
        {
            // Arrange
            var startTime = DateTime.UtcNow.AddHours(-2);
            var duration = TimeSpan.FromHours(1); // Expired 1 hour ago
            var flag = new FeatureFlag
            {
                Name = "test-flag",
                IsEnabled = true,
                Environment = _environment,
                UseTimeBasedActivation = true,
                StartTime = startTime,
                Duration = duration
            };

            // Act
            var result = flag.IsTimeActive();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EffectiveEndTime_WithExplicitEndTime_ReturnsEndTime()
        {
            // Arrange
            var endTime = DateTime.UtcNow.AddHours(1);
            var flag = new FeatureFlag
            {
                EndTime = endTime,
                StartTime = DateTime.UtcNow,
                Duration = TimeSpan.FromHours(2) // Should be ignored
            };

            // Act
            var result = flag.EffectiveEndTime;

            // Assert
            Assert.Equal(endTime, result);
        }

        [Fact]
        public void EffectiveEndTime_WithStartTimeAndDuration_ReturnsCalculatedEndTime()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var duration = TimeSpan.FromHours(2);
            var expectedEndTime = startTime.Add(duration);
            var flag = new FeatureFlag
            {
                StartTime = startTime,
                Duration = duration
            };

            // Act
            var result = flag.EffectiveEndTime;

            // Assert
            Assert.Equal(expectedEndTime, result);
        }

        [Fact]
        public async Task ScheduleActivationAsync_ValidInput_UpdatesFlag()
        {
            // Arrange
            var flag = new FeatureFlag
            {
                Id = Guid.NewGuid(),
                Name = "test-flag",
                IsEnabled = true,
                Environment = _environment
            };

            _featureStoreMock.Setup(s => s.GetAsync("test-flag")).ReturnsAsync(flag);
            _featureStoreMock.Setup(s => s.SetFlagAsync(It.IsAny<FeatureFlag>())).Returns(Task.CompletedTask);

            var startTime = DateTime.UtcNow.AddHours(1);
            var duration = TimeSpan.FromHours(2);

            // Act
            await _scheduler.ScheduleActivationAsync(flag.Name, startTime, duration);

            // Assert - Verify that SetFlagAsync was called with a flag that has the correct properties
            _featureStoreMock.Verify(s => s.SetFlagAsync(It.Is<FeatureFlag>(f => 
                f.UseTimeBasedActivation && 
                f.StartTime.HasValue &&
                f.Duration == duration)), Times.Once);
        }

        [Fact]
        public async Task ScheduleTemporaryActivationAsync_ValidInput_SchedulesFromNow()
        {
            // Arrange
            var flag = new FeatureFlag
            {
                Id = Guid.NewGuid(),
                Name = "test-flag",
                IsEnabled = true,
                Environment = _environment
            };

            _featureStoreMock.Setup(s => s.GetAsync("test-flag")).ReturnsAsync(flag);
            _featureStoreMock.Setup(s => s.SetFlagAsync(It.IsAny<FeatureFlag>())).Returns(Task.CompletedTask);

            var duration = TimeSpan.FromHours(1);
            var beforeScheduling = DateTime.UtcNow;

            // Act
            await _scheduler.ScheduleTemporaryActivationAsync(flag.Name, duration);

            // Assert
            _featureStoreMock.Verify(s => s.SetFlagAsync(It.Is<FeatureFlag>(f => 
                f.UseTimeBasedActivation && 
                f.StartTime >= beforeScheduling && 
                f.Duration == duration)), Times.Once);
        }

        [Fact]
        public async Task GetUpcomingChangesAsync_ReturnsScheduledChanges()
        {
            // Arrange
            var flags = new List<FeatureFlag>
            {
                new FeatureFlag
                {
                    Id = Guid.NewGuid(),
                    Name = "flag1",
                    IsEnabled = true,
                    Environment = _environment,
                    UseTimeBasedActivation = true,
                    StartTime = DateTime.UtcNow.AddHours(2)
                },
                new FeatureFlag
                {
                    Id = Guid.NewGuid(),
                    Name = "flag2",
                    IsEnabled = true,
                    Environment = _environment,
                    UseTimeBasedActivation = true,
                    StartTime = DateTime.UtcNow.AddHours(-1),
                    EndTime = DateTime.UtcNow.AddHours(1)
                }
            };

            _featureStoreMock.Setup(s => s.GetAllFlagsAsync(_environment)).ReturnsAsync(flags);

            // Act
            var upcomingChanges = await _scheduler.GetUpcomingChangesAsync(24);

            // Assert
            Assert.Contains(upcomingChanges, c => c.FlagName == "flag1" && c.ChangeType == ScheduledChangeType.Activation);
            Assert.Contains(upcomingChanges, c => c.FlagName == "flag2" && c.ChangeType == ScheduledChangeType.Deactivation);
        }

        [Fact]
        public async Task RemoveSchedulingAsync_ClearsTimeBasedProperties()
        {
            // Arrange
            var flag = new FeatureFlag
            {
                Id = Guid.NewGuid(),
                Name = "test-flag",
                IsEnabled = true,
                Environment = _environment,
                UseTimeBasedActivation = true,
                StartTime = DateTime.UtcNow.AddHours(1),
                Duration = TimeSpan.FromHours(2),
                TimeZone = "America/New_York"
            };

            _featureStoreMock.Setup(s => s.GetAsync("test-flag")).ReturnsAsync(flag);
            _featureStoreMock.Setup(s => s.SetFlagAsync(It.IsAny<FeatureFlag>())).Returns(Task.CompletedTask);

            // Act
            await _scheduler.RemoveSchedulingAsync(flag.Name);

            // Assert
            _featureStoreMock.Verify(s => s.SetFlagAsync(It.Is<FeatureFlag>(f => 
                !f.UseTimeBasedActivation && 
                f.StartTime == null && 
                f.EndTime == null && 
                f.Duration == null && 
                f.TimeZone == null)), Times.Once);
        }

        [Fact]
        public void IsValidTimeZone_ValidTimeZone_ReturnsTrue()
        {
            // Act & Assert
            Assert.True(_scheduler.IsValidTimeZone("America/New_York"));
            Assert.True(_scheduler.IsValidTimeZone("UTC"));
            Assert.True(_scheduler.IsValidTimeZone("Europe/London"));
        }

        [Fact]
        public void IsValidTimeZone_InvalidTimeZone_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(_scheduler.IsValidTimeZone("Invalid/TimeZone"));
            Assert.False(_scheduler.IsValidTimeZone(""));
            Assert.False(_scheduler.IsValidTimeZone(null!));
        }
    }
}

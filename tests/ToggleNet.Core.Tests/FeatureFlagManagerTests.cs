using System.Threading.Tasks;
using ToggleNet.Core;
using ToggleNet.Core.Entities;
using ToggleNet.Core.Storage;
using Xunit;
using Moq;
using System.Collections.Generic;

namespace ToggleNet.Core.Tests
{
    public class FeatureFlagManagerTests
    {
        [Fact]
        public async Task IsEnabledAsync_ReturnsFalse_WhenFlagDoesNotExist()
        {
            // Arrange
            var storeMock = new Mock<IFeatureStore>();
            storeMock.Setup(s => s.GetAllFlagsAsync(It.IsAny<string>())).ReturnsAsync(new List<FeatureFlag>());
            var manager = new FeatureFlagManager(storeMock.Object, "TestEnv");

            // Act
            var result = await manager.IsEnabledAsync("nonexistent-flag");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsEnabledAsync_ReturnsTrue_WhenFlagIsEnabledAndEnvironmentMatches()
        {
            var storeMock = new Mock<IFeatureStore>();
            storeMock.Setup(s => s.GetAsync("enabled-flag")).ReturnsAsync(new FeatureFlag {
                Name = "enabled-flag",
                IsEnabled = true,
                Environment = "TestEnv",
                RolloutPercentage = 100
            });
            var manager = new FeatureFlagManager(storeMock.Object, "TestEnv");

            var result = await manager.IsEnabledAsync("enabled-flag");

            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_ReturnsFalse_WhenFlagIsDisabled()
        {
            var storeMock = new Mock<IFeatureStore>();
            storeMock.Setup(s => s.GetAsync("disabled-flag")).ReturnsAsync(new FeatureFlag {
                Name = "disabled-flag",
                IsEnabled = false,
                Environment = "TestEnv",
                RolloutPercentage = 100
            });
            var manager = new FeatureFlagManager(storeMock.Object, "TestEnv");

            var result = await manager.IsEnabledAsync("disabled-flag");

            Assert.False(result);
        }

        [Fact]
        public async Task IsEnabledAsync_UserSpecific_ReturnsTrue_WhenUserInRollout()
        {
            var storeMock = new Mock<IFeatureStore>();
            storeMock.Setup(s => s.GetAsync("rollout-flag")).ReturnsAsync(new FeatureFlag {
                Name = "rollout-flag",
                IsEnabled = true,
                Environment = "TestEnv",
                RolloutPercentage = 100
            });
            var manager = new FeatureFlagManager(storeMock.Object, "TestEnv");

            var result = await manager.IsEnabledAsync("rollout-flag", "user-1");

            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_UserSpecific_ReturnsFalse_WhenFlagEnvironmentDoesNotMatch()
        {
            var storeMock = new Mock<IFeatureStore>();
            storeMock.Setup(s => s.GetAsync("flag-env")).ReturnsAsync(new FeatureFlag {
                Name = "flag-env",
                IsEnabled = true,
                Environment = "OtherEnv",
                RolloutPercentage = 100
            });
            var manager = new FeatureFlagManager(storeMock.Object, "TestEnv");

            var result = await manager.IsEnabledAsync("flag-env", "user-1");

            Assert.False(result);
        }
    }
}

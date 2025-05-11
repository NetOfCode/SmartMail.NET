using Xunit;
using Moq;
using FluentAssertions;
using SmartMail.NET.Core.Services;
using SmartMail.NET.Core.Models;
using SmartMail.NET.Core.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SmartMail.NET.Tests
{
    public class EmailProviderManagerTests
    {
        private readonly Mock<IEmailProvider> _mockProvider1;
        private readonly Mock<IEmailProvider> _mockProvider2;
        private readonly EmailProviderManager _providerManager;
        private readonly Mock<ILogger<EmailProviderManager>> _mockLogger;

        public EmailProviderManagerTests()
        {
            _mockProvider1 = new Mock<IEmailProvider>();
            _mockProvider2 = new Mock<IEmailProvider>();

            _mockProvider1.Setup(x => x.Name).Returns("Provider1");
            _mockProvider2.Setup(x => x.Name).Returns("Provider2");

            var options = new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig
                    {
                        Name = "Provider1",
                        Priority = 1,
                        HourlyQuota = 100,
                        DailyQuota = 1000,
                        MonthlyQuota = 10000,
                        IsEnabled = true
                    },
                    new ProviderConfig
                    {
                        Name = "Provider2",
                        Priority = 2,
                        HourlyQuota = 100,
                        DailyQuota = 1000,
                        MonthlyQuota = 10000,
                        IsEnabled = true
                    }
                }
            };

            var mockOptions = new Mock<IOptions<EmailProviderOptions>>();
            mockOptions.Setup(x => x.Value).Returns(options);

            _mockLogger = new Mock<ILogger<EmailProviderManager>>();

            var providers = new List<IEmailProvider> { _mockProvider1.Object, _mockProvider2.Object };
            _providerManager = new EmailProviderManager(providers, mockOptions.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetNextProviderAsync_WithValidQuota_ShouldReturnProvider()
        {
            // Act
            var provider = await _providerManager.GetNextProviderAsync();

            // Assert
            provider.Should().NotBeNull();
            provider.Name.Should().Be("Provider1");
        }

        [Fact]
        public async Task ReportSuccessAsync_ShouldUpdateStats()
        {
            // Arrange
            var provider = await _providerManager.GetNextProviderAsync();

            // Act
            await _providerManager.ReportSuccessAsync(provider);
            await _providerManager.ReportSuccessAsync(provider);

            // Assert
            // The provider should still be available since we haven't exceeded quotas
            var nextProvider = await _providerManager.GetNextProviderAsync();
            nextProvider.Should().NotBeNull();
            nextProvider.Name.Should().Be("Provider1");
        }

        [Fact]
        public async Task GetNextProviderAsync_WhenHourlyQuotaExceeded_ShouldReturnNextProvider()
        {
            // Arrange
            var provider1 = await _providerManager.GetNextProviderAsync();

            // Simulate exceeding hourly quota for first provider
            for (int i = 0; i < 100; i++)
            {
                await _providerManager.ReportSuccessAsync(provider1);
            }

            // Act
            var nextProvider = await _providerManager.GetNextProviderAsync();

            // Assert
            nextProvider.Should().NotBeNull();
            nextProvider.Name.Should().Be("Provider2");
        }

        [Fact]
        public async Task GetNextProviderAsync_WhenDailyQuotaExceeded_ShouldReturnNextProvider()
        {
            // Arrange
            var provider1 = await _providerManager.GetNextProviderAsync();

            // Simulate exceeding daily quota for first provider
            for (int i = 0; i < 1000; i++)
            {
                await _providerManager.ReportSuccessAsync(provider1);
            }

            // Act
            var nextProvider = await _providerManager.GetNextProviderAsync();

            // Assert
            nextProvider.Should().NotBeNull();
            nextProvider.Name.Should().Be("Provider2");
        }

        [Fact]
        public async Task GetNextProviderAsync_WhenMonthlyQuotaExceeded_ShouldReturnNextProvider()
        {
            // Arrange
            var provider1 = await _providerManager.GetNextProviderAsync();

            // Simulate exceeding monthly quota for first provider
            for (int i = 0; i < 10000; i++)
            {
                await _providerManager.ReportSuccessAsync(provider1);
            }

            // Act
            var nextProvider = await _providerManager.GetNextProviderAsync();

            // Assert
            nextProvider.Should().NotBeNull();
            nextProvider.Name.Should().Be("Provider2");
        }

        [Fact]
        public async Task GetNextProviderAsync_WhenProviderDisabled_ShouldSkipProvider()
        {
            // Arrange
            var options = new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig
                    {
                        Name = "Provider1",
                        Priority = 1,
                        IsEnabled = false
                    },
                    new ProviderConfig
                    {
                        Name = "Provider2",
                        Priority = 2,
                        IsEnabled = true
                    }
                }
            };

            var mockOptions = new Mock<IOptions<EmailProviderOptions>>();
            mockOptions.Setup(x => x.Value).Returns(options);

            var providers = new List<IEmailProvider> { _mockProvider1.Object, _mockProvider2.Object };
            var manager = new EmailProviderManager(providers, mockOptions.Object, _mockLogger.Object);

            // Act
            var provider = await manager.GetNextProviderAsync();

            // Assert
            provider.Should().NotBeNull();
            provider.Name.Should().Be("Provider2");
        }

        [Fact]
        public async Task GetNextProviderAsync_WhenNoProvidersAvailable_ShouldThrowException()
        {
            // Arrange
            var options = new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig
                    {
                        Name = "Provider1",
                        Priority = 1,
                        IsEnabled = false
                    },
                    new ProviderConfig
                    {
                        Name = "Provider2",
                        Priority = 2,
                        IsEnabled = false
                    }
                }
            };

            var mockOptions = new Mock<IOptions<EmailProviderOptions>>();
            mockOptions.Setup(x => x.Value).Returns(options);

            var providers = new List<IEmailProvider> { _mockProvider1.Object, _mockProvider2.Object };
            var manager = new EmailProviderManager(providers, mockOptions.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => manager.GetNextProviderAsync());
        }

        [Fact]
        public async Task ReportFailureAsync_ShouldNotAffectQuota()
        {
            // Arrange
            var provider = await _providerManager.GetNextProviderAsync();

            // Act
            await _providerManager.ReportFailureAsync(provider);
            await _providerManager.ReportFailureAsync(provider);

            // Assert
            var nextProvider = await _providerManager.GetNextProviderAsync();
            nextProvider.Should().NotBeNull();
            nextProvider.Name.Should().Be("Provider1"); // Should still use Provider1 as failures don't count towards quota
        }

        [Fact]
        public async Task GetNextProviderAsync_WhenQuotaExceededAndNoOtherProviders_ShouldThrowException()
        {
            // Arrange
            var options = new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig
                    {
                        Name = "Provider1",
                        Priority = 1,
                        DailyQuota = 1000,
                        HourlyQuota = 100,
                        MonthlyQuota = 10000,
                        IsEnabled = true
                    }
                }
            };

            var mockOptions = new Mock<IOptions<EmailProviderOptions>>();
            mockOptions.Setup(x => x.Value).Returns(options);

            var providers = new List<IEmailProvider> { _mockProvider1.Object };
            var manager = new EmailProviderManager(providers, mockOptions.Object, _mockLogger.Object);

            // Act - Exceed the daily quota
            var provider = await manager.GetNextProviderAsync();
            for (int i = 0; i < 1000; i++)
            {
                await manager.ReportSuccessAsync(provider);
            }

            // Assert - Should throw exception with detailed quota information
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => manager.GetNextProviderAsync());
            exception.Message.Should().Contain("No available email providers. Quota exceeded for: Provider1");
            exception.Message.Should().Contain("Daily: 1000/1000");
            exception.Message.Should().Contain("Please add more providers or increase the quota limits for existing providers");
        }

        [Fact]
        public async Task GetNextProviderAsync_WhenProviderReachesMultipleQuotas_ShouldSwitchToNextProvider()
        {
            // Arrange
            var options = new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig
                    {
                        Name = "Provider1",
                        Priority = 1,
                        HourlyQuota = 50,
                        DailyQuota = 200,
                        MonthlyQuota = 1000,
                        IsEnabled = true
                    },
                    new ProviderConfig
                    {
                        Name = "Provider2",
                        Priority = 2,
                        HourlyQuota = 100,
                        DailyQuota = 500,
                        MonthlyQuota = 2000,
                        IsEnabled = true
                    }
                }
            };

            var mockOptions = new Mock<IOptions<EmailProviderOptions>>();
            mockOptions.Setup(x => x.Value).Returns(options);

            var providers = new List<IEmailProvider> { _mockProvider1.Object, _mockProvider2.Object };
            var manager = new EmailProviderManager(providers, mockOptions.Object, _mockLogger.Object);

            // Act - Exceed hourly quota for first provider
            var provider1 = await manager.GetNextProviderAsync();
            for (int i = 0; i < 50; i++)
            {
                await manager.ReportSuccessAsync(provider1);
            }

            // Assert - Should switch to Provider2 after hourly quota is exceeded
            var nextProvider = await manager.GetNextProviderAsync();
            nextProvider.Should().NotBeNull();
            nextProvider.Name.Should().Be("Provider2");

            // Act - Exceed daily quota for first provider
            for (int i = 0; i < 150; i++) // Additional 150 to reach daily quota
            {
                await manager.ReportSuccessAsync(provider1);
            }

            // Assert - Should still use Provider2 as Provider1 has exceeded daily quota
            nextProvider = await manager.GetNextProviderAsync();
            nextProvider.Should().NotBeNull();
            nextProvider.Name.Should().Be("Provider2");

            // Act - Exceed monthly quota for first provider
            for (int i = 0; i < 800; i++) // Additional 800 to reach monthly quota
            {
                await manager.ReportSuccessAsync(provider1);
            }

            // Assert - Should still use Provider2 as Provider1 has exceeded all quotas
            nextProvider = await manager.GetNextProviderAsync();
            nextProvider.Should().NotBeNull();
            nextProvider.Name.Should().Be("Provider2");

            // Verify Provider2 is still available and working
            var result = await manager.GetNextProviderAsync();
            result.Should().NotBeNull();
            result.Name.Should().Be("Provider2");
        }
    }
} 
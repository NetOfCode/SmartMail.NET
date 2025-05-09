using Xunit;
using Moq;
using FluentAssertions;
using SendMail.NET.Core.Services;
using SendMail.NET.Core.Models;
using SendMail.NET.Core.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SendMail.NET.Tests
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
    }
} 
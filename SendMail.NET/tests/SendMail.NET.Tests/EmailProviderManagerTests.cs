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

            var smtpSettings1 = new SmtpProviderSettings(
                host: "smtp.test1.com",
                port: 587,
                username: "test1@test.com",
                password: "test-password1",
                defaultFrom: "test1@test.com",
                enableSsl: true
            );

            var smtpSettings2 = new SmtpProviderSettings(
                host: "smtp.test2.com",
                port: 587,
                username: "test2@test.com",
                password: "test-password2",
                defaultFrom: "test2@test.com",
                enableSsl: true
            );

            var options = new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig("Provider1", 1, smtpSettings1)
                        .WithHourlyQuota(100)
                        .WithDailyQuota(1000)
                        .WithMonthlyQuota(10000)
                        .WithEnabled(true),
                    new ProviderConfig("Provider2", 2, smtpSettings2)
                        .WithHourlyQuota(100)
                        .WithDailyQuota(1000)
                        .WithMonthlyQuota(10000)
                        .WithEnabled(true)
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
            var smtpSettings1 = new SmtpProviderSettings(
                host: "smtp.test1.com",
                port: 587,
                username: "test1@test.com",
                password: "test-password1",
                defaultFrom: "test1@test.com",
                enableSsl: true
            );

            var smtpSettings2 = new SmtpProviderSettings(
                host: "smtp.test2.com",
                port: 587,
                username: "test2@test.com",
                password: "test-password2",
                defaultFrom: "test2@test.com",
                enableSsl: true
            );

            var options = new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig("Provider1", 1, smtpSettings1)
                        .WithEnabled(false),
                    new ProviderConfig("Provider2", 2, smtpSettings2)
                        .WithEnabled(true)
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
            var smtpSettings1 = new SmtpProviderSettings(
                host: "smtp.test1.com",
                port: 587,
                username: "test1@test.com",
                password: "test-password1",
                defaultFrom: "test1@test.com",
                enableSsl: true
            );

            var smtpSettings2 = new SmtpProviderSettings(
                host: "smtp.test2.com",
                port: 587,
                username: "test2@test.com",
                password: "test-password2",
                defaultFrom: "test2@test.com",
                enableSsl: true
            );

            var options = new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig("Provider1", 1, smtpSettings1)
                        .WithEnabled(false),
                    new ProviderConfig("Provider2", 2, smtpSettings2)
                        .WithEnabled(false)
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
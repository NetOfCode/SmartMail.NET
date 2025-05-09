using Xunit;
using FluentAssertions;
using SendMail.NET.Core.Providers;
using SendMail.NET.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net.Mail;

namespace SendMail.NET.Tests
{
    public class SmtpEmailProviderTests
    {
        private readonly SmtpEmailProvider _smtpProvider;
        private readonly Mock<ILogger<SmtpEmailProvider>> _mockLogger;
        private readonly Mock<ISmtpClient> _mockSmtpClient;

        public SmtpEmailProviderTests()
        {
            var smtpSettings = new SmtpProviderSettings(
                host: "smtp.test.com",
                port: 587,
                username: "test@test.com",
                password: "test-password",
                defaultFrom: "test@test.com",
                enableSsl: true
            );

            var options = new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig("SMTP", 1, smtpSettings)
                        .WithHourlyQuota(100)
                        .WithDailyQuota(1000)
                        .WithMonthlyQuota(10000)
                        .WithEnabled(true)
                }
            };

            _mockLogger = new Mock<ILogger<SmtpEmailProvider>>();
            _mockSmtpClient = new Mock<ISmtpClient>();
            var mockOptions = new Mock<IOptions<EmailProviderOptions>>();
            mockOptions.Setup(x => x.Value).Returns(options);

            _smtpProvider = new SmtpEmailProvider(mockOptions.Object, _mockLogger.Object, _mockSmtpClient.Object);
        }

        [Fact]
        public void Constructor_WithValidOptions_ShouldInitialize()
        {
            // Assert
            _smtpProvider.Should().NotBeNull();
            _smtpProvider.Name.Should().Be("SMTP");
        }

        [Fact]
        public async Task SendAsync_WhenMessageValid_ShouldSendEmail()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "recipient@test.com",
                Subject = "Test Subject",
                Body = "Test Body",
                IsHtml = false
            };

            _mockSmtpClient
                .Setup(x => x.SendMailAsync(It.IsAny<MailMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _smtpProvider.SendAsync(message);

            // Assert
            result.Success.Should().BeTrue();
            result.MessageId.Should().NotBeNullOrEmpty();
            _mockSmtpClient.Verify(x => x.SendMailAsync(It.IsAny<MailMessage>()), Times.Once);
        }

        [Fact]
        public async Task SendAsync_WhenSendFails_ShouldReturnFailure()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "recipient@test.com",
                Subject = "Test Subject",
                Body = "Test Body",
                IsHtml = false
            };

            _mockSmtpClient
                .Setup(x => x.SendMailAsync(It.IsAny<MailMessage>()))
                .ThrowsAsync(new SmtpException("Test error"));

            // Act
            var result = await _smtpProvider.SendAsync(message);

            // Assert
            result.Success.Should().BeFalse();
            result.Error.Should().Be("Test error");
        }

        [Fact]
        public async Task SendAsync_WithHtmlMessage_ShouldSetContentType()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "recipient@test.com",
                Subject = "Test Subject",
                Body = "<h1>Test Body</h1>",
                IsHtml = true
            };

            _mockSmtpClient.Setup(x => x.SendMailAsync(It.IsAny<MailMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _smtpProvider.SendAsync(message);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.MessageId.Should().NotBeNullOrEmpty();
            _mockSmtpClient.Verify(x => x.SendMailAsync(It.Is<MailMessage>(m =>
                m.IsBodyHtml == true)), Times.Once);
        }

        [Fact]
        public async Task SendAsync_WithAttachments_ShouldIncludeAttachments()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "recipient@test.com",
                Subject = "Test Subject",
                Body = "Test Body",
                Attachments = new List<EmailAttachment>
                {
                    new EmailAttachment
                    {
                        FileName = "test.txt",
                        ContentType = "text/plain",
                        Content = new byte[] { 1, 2, 3 }
                    }
                }
            };

            _mockSmtpClient.Setup(x => x.SendMailAsync(It.IsAny<MailMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _smtpProvider.SendAsync(message);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.MessageId.Should().NotBeNullOrEmpty();
            _mockSmtpClient.Verify(x => x.SendMailAsync(It.Is<MailMessage>(m =>
                m.Attachments.Count == 1 &&
                m.Attachments[0].Name == "test.txt")), Times.Once);
        }

        [Fact]
        public async Task SendAsync_WithInvalidSettings_ShouldFail()
        {
            // Arrange
            var smtpSettings = new SmtpProviderSettings(
                host: "invalid-host",
                port: 587,
                username: "test@test.com",
                password: "test-password",
                defaultFrom: "test@test.com",
                enableSsl: true
            );

            var options = new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig("SMTP", 1, smtpSettings)
                        .WithEnabled(true)
                }
            };

            var mockOptions = new Mock<IOptions<EmailProviderOptions>>();
            mockOptions.Setup(x => x.Value).Returns(options);
            var provider = new SmtpEmailProvider(mockOptions.Object, _mockLogger.Object, _mockSmtpClient.Object);

            var message = new EmailMessage
            {
                To = "recipient@test.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            _mockSmtpClient.Setup(x => x.SendMailAsync(It.IsAny<MailMessage>()))
                .ThrowsAsync(new SmtpException("Invalid host"));

            // Act
            var result = await provider.SendAsync(message);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }
    }
} 
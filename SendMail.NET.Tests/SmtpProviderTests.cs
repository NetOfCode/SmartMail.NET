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
            var options = new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig
                    {
                        Name = "SMTP",
                        Priority = 1,
                        HourlyQuota = 100,
                        DailyQuota = 1000,
                        MonthlyQuota = 10000,
                        Settings = new Dictionary<string, string>
                        {
                            { "Host", "smtp.test.com" },
                            { "Port", "587" },
                            { "Username", "test@test.com" },
                            { "Password", "test-password" },
                            { "EnableSsl", "true" },
                            { "DefaultFrom", "test@test.com" }
                        }
                    }
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
        public async Task SendAsync_WithValidMessage_ShouldSucceed()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "recipient@test.com",
                Subject = "Test Subject",
                Body = "Test Body",
                IsHtml = false,
                Cc = new List<string> { "cc@test.com" },
                Bcc = new List<string> { "bcc@test.com" }
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
                m.To[0].Address == message.To &&
                m.CC[0].Address == message.Cc[0] &&
                m.Bcc[0].Address == message.Bcc[0] &&
                m.Subject == message.Subject &&
                m.Body == message.Body)), Times.Once);
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
            var options = new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig
                    {
                        Name = "SMTP",
                        Settings = new Dictionary<string, string>
                        {
                            { "Host", "invalid-host" },
                            { "Port", "587" },
                            { "Username", "test@test.com" },
                            { "Password", "test-password" },
                            { "EnableSsl", "true" },
                            { "DefaultFrom", "test@test.com" }
                        }
                    }
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
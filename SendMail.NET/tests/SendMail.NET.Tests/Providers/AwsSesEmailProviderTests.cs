using System;
using System.Threading.Tasks;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SendMail.NET.Core.Models;
using SendMail.NET.Core.Providers;
using SendMail.NET.Core.Pipeline;
using Xunit;

namespace SendMail.NET.Tests.Providers
{
    public class AwsSesEmailProviderTests
    {
        private readonly Mock<ILogger<AwsSesEmailProvider>> _loggerMock;
        private readonly Mock<IAmazonSimpleEmailService> _sesClientMock;
        private readonly EmailProviderOptions _options;
        private readonly AwsSesEmailProvider _provider;

        public AwsSesEmailProviderTests()
        {
            _loggerMock = new Mock<ILogger<AwsSesEmailProvider>>();
            _sesClientMock = new Mock<IAmazonSimpleEmailService>();
            _options = new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig
                    {
                        Name = "AWS SES",
                        Settings = new Dictionary<string, string>
                        {
                            { "Region", "us-east-1" },
                            { "AccessKey", "test-access-key" },
                            { "SecretKey", "test-secret-key" },
                            { "DefaultFrom", "test@example.com" }
                        }
                    }
                }
            };

            _provider = new AwsSesEmailProvider(
                Options.Create(_options),
                _loggerMock.Object,
                _sesClientMock.Object
            );
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                new AwsSesEmailProvider(null, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                new AwsSesEmailProvider(Options.Create(_options), null));
        }

        [Fact]
        public void Name_ReturnsProviderName()
        {
            Assert.Equal("AWS SES", _provider.Name);
        }

        [Fact]
        public async Task SendAsync_WithValidMessage_SendsEmail()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "recipient@example.com",
                Subject = "Test Subject",
                Body = "Test Body",
                IsHtml = false
            };

            _sesClientMock
                .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
                .ReturnsAsync(new SendEmailResponse { MessageId = "test-message-id" });

            // Act
            var result = await _provider.SendAsync(message);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("test-message-id", result.MessageId);
            _sesClientMock.Verify(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), default), Times.Once);
        }

        [Fact]
        public async Task SendAsync_WithHtmlMessage_SendsHtmlEmail()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "recipient@example.com",
                Subject = "Test Subject",
                Body = "<p>Test Body</p>",
                IsHtml = true
            };

            _sesClientMock
                .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
                .ReturnsAsync(new SendEmailResponse { MessageId = "test-message-id" });

            // Act
            var result = await _provider.SendAsync(message);

            // Assert
            Assert.True(result.Success);
            _sesClientMock.Verify(x => x.SendEmailAsync(
                It.Is<SendEmailRequest>(req => 
                    req.Message.Body.Html != null && 
                    req.Message.Body.Text == null), 
                default), 
                Times.Once);
        }

        [Fact]
        public async Task SendAsync_WhenSesThrowsException_ReturnsFailure()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "recipient@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            _sesClientMock
                .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
                .ThrowsAsync(new Exception("SES error"));

            // Act
            var result = await _provider.SendAsync(message);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SES error", result.Error);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendAsync_WithCustomFrom_UsesCustomFrom()
        {
            // Arrange
            var message = new EmailMessage
            {
                From = "custom@example.com",
                To = "recipient@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            _sesClientMock
                .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
                .ReturnsAsync(new SendEmailResponse { MessageId = "test-message-id" });

            // Act
            var result = await _provider.SendAsync(message);

            // Assert
            Assert.True(result.Success);
            _sesClientMock.Verify(x => x.SendEmailAsync(
                It.Is<SendEmailRequest>(req => req.Source == "custom@example.com"),
                default),
                Times.Once);
        }

        [Fact]
        public void Constructor_WithNullOptionsValue_ThrowsArgumentException()
        {
            var options = new Mock<IOptions<EmailProviderOptions>>();
            options.Setup(x => x.Value).Returns((EmailProviderOptions)null);

            Assert.Throws<ArgumentException>(() => 
                new AwsSesEmailProvider(options.Object, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithEmptyProviders_ThrowsArgumentException()
        {
            var options = new Mock<IOptions<EmailProviderOptions>>();
            options.Setup(x => x.Value).Returns(new EmailProviderOptions { Providers = new List<ProviderConfig>() });

            Assert.Throws<ArgumentException>(() => 
                new AwsSesEmailProvider(options.Object, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithMissingDefaultFrom_ThrowsArgumentException()
        {
            var options = new Mock<IOptions<EmailProviderOptions>>();
            options.Setup(x => x.Value).Returns(new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig
                    {
                        Name = "AWS SES",
                        Settings = new Dictionary<string, string>
                        {
                            { "Region", "us-east-1" },
                            { "AccessKey", "test-access-key" },
                            { "SecretKey", "test-secret-key" }
                        }
                    }
                }
            });

            Assert.Throws<ArgumentException>(() => 
                new AwsSesEmailProvider(options.Object, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithMissingAwsSettings_ThrowsArgumentException()
        {
            var options = new Mock<IOptions<EmailProviderOptions>>();
            options.Setup(x => x.Value).Returns(new EmailProviderOptions
            {
                Providers = new List<ProviderConfig>
                {
                    new ProviderConfig
                    {
                        Name = "AWS SES",
                        Settings = new Dictionary<string, string>
                        {
                            { "DefaultFrom", "test@example.com" }
                        }
                    }
                }
            });

            Assert.Throws<ArgumentException>(() => 
                new AwsSesEmailProvider(options.Object, _loggerMock.Object));
        }
    }
} 
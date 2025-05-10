using Xunit;
using Moq;
using FluentAssertions;
using SendMail.NET.Core.Services;
using SendMail.NET.Core.Models;
using SendMail.NET.Core.Providers;
using SendMail.NET.Core.Pipeline;
using SendMail.NET.Core.Pipeline.Steps;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SendMail.NET.Tests
{
    public class SendMailServiceTests
    {
        private readonly Mock<IEmailProvider> _mockProvider;
        private readonly Mock<IEmailProviderManager> _mockProviderManager;
        private readonly Mock<ILogger<SendMailService>> _mockLogger;
        private readonly Mock<ILogger<EmailPipeline>> _mockPipelineLogger;
        private readonly Mock<ILogger<SendingStep>> _mockSendingStepLogger;
        private readonly SendMailService _sendMailService;

        public SendMailServiceTests()
        {
            _mockProvider = new Mock<IEmailProvider>();
            _mockProviderManager = new Mock<IEmailProviderManager>();
            _mockLogger = new Mock<ILogger<SendMailService>>();
            _mockPipelineLogger = new Mock<ILogger<EmailPipeline>>();
            _mockSendingStepLogger = new Mock<ILogger<SendingStep>>();

            _mockProvider.Setup(x => x.Name).Returns("TestProvider");
            _mockProviderManager.Setup(x => x.GetNextProviderAsync())
                .ReturnsAsync(_mockProvider.Object);

            var sendingStep = new SendingStep(_mockSendingStepLogger.Object);
            var pipeline = new EmailPipeline(new List<IEmailPipelineStep> { sendingStep }, _mockPipelineLogger.Object);
            _sendMailService = new SendMailService(pipeline, _mockProviderManager.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SendAsync_ValidEmail_ShouldSucceed()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body",
                IsHtml = false
            };

            var expectedResult = new SendResult
            {
                Success = true,
                MessageId = "test-message-id"
            };

            _mockProvider.Setup(x => x.SendAsync(It.IsAny<EmailMessage>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sendMailService.SendAsync(message);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.MessageId.Should().Be("test-message-id");
            _mockProvider.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => 
                m.To == message.To && 
                m.Subject == message.Subject && 
                m.Body == message.Body)), Times.Once);
            _mockProviderManager.Verify(x => x.ReportSuccessAsync(_mockProvider.Object), Times.Once);
        }

        [Fact]
        public async Task SendAsync_InvalidEmail_ShouldFail()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "invalid-email",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            _mockProvider.Setup(x => x.SendAsync(It.IsAny<EmailMessage>()))
                .ThrowsAsync(new System.Exception("Failed to send email: Invalid email address"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.Exception>(() => _sendMailService.SendAsync(message));
            exception.Message.Should().Be("Failed to send email: Invalid email address");
            _mockProviderManager.Verify(x => x.ReportFailureAsync(_mockProvider.Object), Times.Once);
        }

        [Fact]
        public async Task SendAsync_ProviderFailure_ShouldThrowException()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            _mockProvider.Setup(x => x.SendAsync(It.IsAny<EmailMessage>()))
                .ThrowsAsync(new System.Exception("Provider error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.Exception>(() => _sendMailService.SendAsync(message));
            exception.Message.Should().Be("Provider error");
            _mockProviderManager.Verify(x => x.ReportFailureAsync(_mockProvider.Object), Times.Once);
        }

        [Fact]
        public async Task SendAsync_WithAttachments_ShouldIncludeAttachments()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "test@example.com",
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

            var expectedResult = new SendResult
            {
                Success = true,
                MessageId = "test-message-id"
            };

            _mockProvider.Setup(x => x.SendAsync(It.IsAny<EmailMessage>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sendMailService.SendAsync(message);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _mockProvider.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => 
                m.Attachments.Count == 1 && 
                m.Attachments[0].FileName == "test.txt")), Times.Once);
            _mockProviderManager.Verify(x => x.ReportSuccessAsync(_mockProvider.Object), Times.Once);
        }
    }
} 
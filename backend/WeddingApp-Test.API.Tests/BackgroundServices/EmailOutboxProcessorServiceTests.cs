using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.Interfaces.Email;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Email;

namespace WeddingApp_Test.API.Tests.BackgroundServices;

public class EmailOutboxProcessorServiceTests
{
    #region Setup
    private static EmailOptions DefaultOptions => new()
    {
        RetryDelayMinutes = [1, 5, 30, 360, 1440],
        MaxAttempts = 4
    };

    private static EmailOutboxProcessorService Build(Mock<IEmailOutboxRepository> repo,
        Mock<IEmailSender> sender) => new(repo.Object, sender.Object, Options.Create(DefaultOptions), NullLogger<EmailOutboxProcessorService>.Instance);

    private static EmailOutbox PendingOutbox(int attempts = 0) => new()
    {
        Id = Guid.NewGuid(),
        ToEmail = "a@b.com",
        ToName = "A",
        Subject = "Hi",
        HtmlBody = "<p>hi</p>",
        Status = EmailStatus.Pending,
        AttemptCount = attempts,
        CreatedAt = DateTime.UtcNow
    };
    #endregion
    
    [Fact]
    public async Task PendingRecord_SenderSucceeds_MarkedSent()
    {
        // Arrange
        var repo = new Mock<IEmailOutboxRepository>();
        var sender = new Mock<IEmailSender>();
        var outbox = PendingOutbox();
        
        repo.Setup(r => r.GetByIdAsync(outbox.Id)).ReturnsAsync(outbox);
        sender.Setup(s => s.SendAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())
        ).ReturnsAsync(true);

        // Act
        await Build(repo, sender).ProcessByIdAsync(outbox.Id, default);
        
        // Assert
        Assert.Equal(EmailStatus.Sent, outbox.Status);
        Assert.Equal(1, outbox.AttemptCount);
        repo.Verify(r => r.Update(outbox), Times.Once);
    }

    [Fact]
    public async Task PendingRecord_SenderFails_IncreasesAttemptAndSchedulesRetry()
    {
        // Arrange
        var repo = new Mock<IEmailOutboxRepository>();
        var sender = new Mock<IEmailSender>();
        var outbox = PendingOutbox(attempts: 0);
        
        repo.Setup(r => r.GetByIdAsync(outbox.Id)).ReturnsAsync(outbox);
        sender.Setup(s => s.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())
        ).ReturnsAsync(false);
        
        // Act
        var nextRetryAt = DateTime.UtcNow.AddMinutes(1);
        await Build(repo, sender).ProcessByIdAsync(outbox.Id, default);
        
        // Assert
        Assert.Equal(EmailStatus.Pending, outbox.Status); // still Pending
        Assert.Equal(1, outbox.AttemptCount);
        Assert.NotNull(outbox.NextRetryAt);
        // First retry uses tier 0 = 1 minutes; must be scheduled roughly 1 minutes from now
        Assert.True(outbox.NextRetryAt > nextRetryAt);
    }
    
    [Fact]
    public async Task AttemptCountReachesMax_MarkedFailed()
    {
        // Arrange
        var repo = new Mock<IEmailOutboxRepository>();
        var sender = new Mock<IEmailSender>();
        var outbox = PendingOutbox(attempts: 3); // MaxAttempts = 4, so this is the last attempt

        repo.Setup(r => r.GetByIdAsync(outbox.Id)).ReturnsAsync(outbox);
        sender.Setup(s => s.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())
        ).ReturnsAsync(false);

        // Act
        await Build(repo, sender).ProcessByIdAsync(outbox.Id, default);

        // Assert
        Assert.Equal(EmailStatus.Failed, outbox.Status);
        Assert.Equal(4, outbox.AttemptCount);
    }
    
    [Fact]
    public async Task RecordAlreadySent_NoSendAttempt()
    {
        // Arrange
        var repo = new Mock<IEmailOutboxRepository>();
        var sender = new Mock<IEmailSender>();

        var outbox = PendingOutbox();
        outbox.Status = EmailStatus.Sent; // already sent

        repo.Setup(r => r.GetByIdAsync(outbox.Id)).ReturnsAsync(outbox);

        // Act
        await Build(repo, sender).ProcessByIdAsync(outbox.Id, default);

        // Assert
        sender.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RecordNull_NoCrash()
    {
        // Arrange
        var repo = new Mock<IEmailOutboxRepository>();
        var sender = new Mock<IEmailSender>();
        var guid = Guid.NewGuid();

        repo.Setup(r => r.GetByIdAsync(guid)).ReturnsAsync((EmailOutbox?)null);

        // Act
        // Should not throw
        await Build(repo, sender).ProcessByIdAsync(guid, default);

        // Assert
        sender.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
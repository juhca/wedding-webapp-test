using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Application.Interfaces.Email;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Infrastructure.Email;

namespace WeddingApp_Test.API.Tests.Email;

/// <summary>
/// Unit tests for <see cref="EmailDispatchService"/>.
/// Verifies template resolution, outbox creation, send-cap enforcement,
/// rendering, and graceful error handling.
/// </summary>
[Trait("Category", "EmailDispatchService Unit Tests")]
public class EmailDispatchServiceTests
{
    #region Setup
    /// <summary>
    /// Creates a minimal <see cref="User"/> with a valid email address.
    /// </summary>
    /// <param name="email">The email address to assign; defaults to <c>a@b.com</c>.</param>
    private static User MakeUser(string email = "a@b.com") => new()
    {
        Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Smith", Email = email
    };

    /// <summary>
    /// Creates an active <see cref="EmailTemplate"/> with a Liquid subject and HTML body.
    /// </summary>
    /// <param name="trigger">The trigger name the template responds to.</param>
    /// <param name="maxSends">
    /// Optional per-user send cap. Pass <c>null</c> (default) for unlimited sends.
    /// </param>
    private static EmailTemplate MakeTemplate(string trigger = "test.event", int? maxSends = null) => new()
    {
        Id = Guid.NewGuid(),
        TriggerName = trigger,
        Name = "Test Template",
        SubjectTemplate = "Hi {{ User.FirstName }}",
        HtmlBodyTemplate = "<p>Hello</p>",
        PlainTextBodyTemplate = null,
        IsActive = true,
        MaxSendsPerUser = maxSends
    };

    /// <summary>
    /// Constructs an <see cref="EmailDispatchService"/> with optional mock overrides.
    /// Any dependency not supplied is replaced with a sensible default:
    /// <list type="bullet">
    ///   <item>The renderer echoes its input template string unchanged.</item>
    ///   <item>The wedding repository returns an empty <see cref="WeddingInfo"/>.</item>
    /// </list>
    /// </summary>
    private EmailDispatchService BuildService(Mock<IEmailTemplateRepository>? templateRepo = null, Mock<IEmailOutboxRepository>? outboxRepo = null,
        Mock<IEmailSendLogRepository>? logRepo = null, Mock<IEmailEventChannel>? channel = null, Mock<ILiquidRenderer>? renderer = null,
        Mock<IWeddingInfoRepository>? weddingRepo = null)
    {
        templateRepo ??= new Mock<IEmailTemplateRepository>();
        outboxRepo ??= new Mock<IEmailOutboxRepository>();
        logRepo ??= new Mock<IEmailSendLogRepository>();
        channel ??= new Mock<IEmailEventChannel>();

        if (renderer is null)
        {
            renderer = new Mock<ILiquidRenderer>();
            renderer.Setup(r => r.RenderAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object?>>()))
                    .ReturnsAsync((string t, Dictionary<string, object?> _) => t);
        }

        if (weddingRepo is null)
        {
            weddingRepo = new Mock<IWeddingInfoRepository>();
            weddingRepo.Setup(r => r.GetWeddingInfoAsync()).ReturnsAsync(new WeddingInfo());
        }

        return new EmailDispatchService(templateRepo.Object, outboxRepo.Object, logRepo.Object, channel.Object, renderer.Object, weddingRepo.Object, 
            NullLogger<EmailDispatchService>.Instance);
    }
    #endregion
    
    #region Outbox creation & publishing
    [Fact]
    public async Task OneTemplate_CreatesOutboxAndPublishes()
    {
        // Arrange
        var templateRepo = new Mock<IEmailTemplateRepository>();
        var outboxRepo   = new Mock<IEmailOutboxRepository>();
        var logRepo      = new Mock<IEmailSendLogRepository>();
        var channel      = new Mock<IEmailEventChannel>();
        var template     = MakeTemplate();

        templateRepo.Setup(r => r.GetActiveByTriggerAsync("test.event", It.IsAny<Guid>()))
            .ReturnsAsync([template]);

        logRepo.Setup(r => r.CountDispatchedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(0);

        var svc = BuildService(templateRepo, outboxRepo, logRepo, channel);

        // Act
        await svc.DispatchEventAsync("test.event", MakeUser(), new(), default);

        // Assert
        outboxRepo.Verify(r => r.AddAsync(It.IsAny<EmailOutbox>()), Times.Once);
        channel.Verify(c => c.PublishAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task TwoTemplates_CreatesTwoOutboxRecords()
    {
        // Arrange
        var templateRepo = new Mock<IEmailTemplateRepository>();
        var outboxRepo = new Mock<IEmailOutboxRepository>();

        templateRepo.Setup(r => r.GetActiveByTriggerAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync([MakeTemplate(), MakeTemplate()]);

        var svc = BuildService(templateRepo, outboxRepo);

        // Act
        await svc.DispatchEventAsync("test.event", MakeUser(), new(), default);

        // Assert
        outboxRepo.Verify(r => r.AddAsync(It.IsAny<EmailOutbox>()), Times.Exactly(2));
    }
    
    [Fact]
    public async Task NoTemplates_NothingCreated()
    {
        // Arrange
        var templateRepo = new Mock<IEmailTemplateRepository>();
        var outboxRepo = new Mock<IEmailOutboxRepository>();

        templateRepo.Setup(r => r.GetActiveByTriggerAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var svc = BuildService(templateRepo, outboxRepo);

        // Act
        await svc.DispatchEventAsync("test.event", MakeUser(), new(), default);

        // Assert
        outboxRepo.Verify(r => r.AddAsync(It.IsAny<EmailOutbox>()), Times.Never);
    }
    #endregion
    
    #region Per-user send cap (MaxSendsPerUser)
    [Fact]
    public async Task MaxSendsPerUser1_AlreadySent1_Skipped()
    {
        // Arrange
        var templateRepo = new Mock<IEmailTemplateRepository>();
        var outboxRepo = new Mock<IEmailOutboxRepository>();
        var logRepo = new Mock<IEmailSendLogRepository>();
        var template = MakeTemplate(maxSends: 1);

        templateRepo.Setup(r => r.GetActiveByTriggerAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync([template]);

        logRepo.Setup(r => r.CountDispatchedAsync(template.Id, It.IsAny<Guid>()))
            .ReturnsAsync(1);

        var svc = BuildService(templateRepo, outboxRepo, logRepo);

        // Act
        await svc.DispatchEventAsync("test.event", MakeUser(), new(), default);

        // Assert
        outboxRepo.Verify(r => r.AddAsync(It.IsAny<EmailOutbox>()), Times.Never);
    }
    
    [Fact]
    public async Task MaxSendsPerUserNull_AlwaysSends()
    {
        // Arrange
        var templateRepo = new Mock<IEmailTemplateRepository>();
        var outboxRepo = new Mock<IEmailOutboxRepository>();
        var logRepo = new Mock<IEmailSendLogRepository>();
        var template = MakeTemplate(maxSends: null);

        templateRepo.Setup(r => r.GetActiveByTriggerAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync([template]);

        var svc = BuildService(templateRepo, outboxRepo, logRepo);

        // Act
        await svc.DispatchEventAsync("test.event", MakeUser(), new(), default);

        // Assert
        outboxRepo.Verify(r => r.AddAsync(It.IsAny<EmailOutbox>()), Times.Once);
    }
    #endregion
    
    #region Guard clauses
    [Fact]
    public async Task UserNoEmail_NothingCreated()
    {
        // Arrange
        var outboxRepo = new Mock<IEmailOutboxRepository>();
        var svc = BuildService(outboxRepo: outboxRepo);

        // Act
        await svc.DispatchEventAsync("test.event", MakeUser(email: ""), new(), default);

        // Assert
        outboxRepo.Verify(r => r.AddAsync(It.IsAny<EmailOutbox>()), Times.Never);
    }
    #endregion

    #region Rendering
    [Fact]
    public async Task RenderedSubjectAndBody_StoredInOutbox()
    {
        // Arrange
        var templateRepo = new Mock<IEmailTemplateRepository>();
        var outboxRepo = new Mock<IEmailOutboxRepository>();
        var renderer = new Mock<ILiquidRenderer>();
        var template = MakeTemplate();

        templateRepo.Setup(r => r.GetActiveByTriggerAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync([template]);

        renderer.Setup(r => r.RenderAsync(template.SubjectTemplate, It.IsAny<Dictionary<string, object?>>()))
                .ReturnsAsync("Hi Anna");
        renderer.Setup(r => r.RenderAsync(template.HtmlBodyTemplate, It.IsAny<Dictionary<string, object?>>()))
                .ReturnsAsync("<p>Hello Anna</p>");

        EmailOutbox? captured = null;
        outboxRepo.Setup(r => r.AddAsync(It.IsAny<EmailOutbox>()))
                  .Callback<EmailOutbox>(o => captured = o);

        var svc = BuildService(templateRepo, outboxRepo, renderer: renderer);

        // Act
        await svc.DispatchEventAsync("test.event", MakeUser(), new(), default);

        // Assert
        Assert.NotNull(captured);
        Assert.Equal("Hi Anna", captured.Subject);
        Assert.Equal("<p>Hello Anna</p>", captured.HtmlBody);
    }

    [Fact]
    public async Task RenderThrows_SkipsTemplate_DoesNotThrow()
    {
        // Arrange
        var templateRepo = new Mock<IEmailTemplateRepository>();
        var outboxRepo = new Mock<IEmailOutboxRepository>();
        var renderer = new Mock<ILiquidRenderer>();

        templateRepo.Setup(r => r.GetActiveByTriggerAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync([MakeTemplate()]);

        renderer.Setup(r => r.RenderAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object?>>()))
                .ThrowsAsync(new InvalidOperationException("bad template"));

        var svc = BuildService(templateRepo, outboxRepo, renderer: renderer);

        // Act — errors are logged and the template is skipped, no exception should surface
        await svc.DispatchEventAsync("test.event", MakeUser(), new(), default);

        // Assert
        outboxRepo.Verify(r => r.AddAsync(It.IsAny<EmailOutbox>()), Times.Never);
    }
    #endregion
}
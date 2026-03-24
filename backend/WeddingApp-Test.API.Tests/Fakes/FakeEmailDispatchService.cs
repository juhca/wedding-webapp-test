using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.API.Tests.Fakes;

/// <summary>
/// No-op email dispatch service used in integration tests to prevent event-driven emails from being sent.
/// </summary>
public class FakeEmailDispatchService : IEmailDispatchService
{
    public Task DispatchEventAsync(string eventName, User triggeredBy, Dictionary<string, object?> context, CancellationToken ct = default) => Task.CompletedTask;
}

using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.Interfaces;

/// <summary>
/// Handles OnEvent email triggers.
/// When a domain event fires (e.g. RsvpSubmitted), call DispatchEventAsync to look up
/// matching active templates and send immediately.
/// </summary>
public interface IEmailDispatchService
{
    /// <summary>
    /// Finds all active OnEvent templates matching <paramref name="eventName"/>,
    /// builds context, renders Liquid, and sends to the appropriate audience.
    /// </summary>
    Task DispatchEventAsync(
        string eventName,
        User triggeringUser,
        Dictionary<string, object?> extraContext,
        CancellationToken ct = default);
}

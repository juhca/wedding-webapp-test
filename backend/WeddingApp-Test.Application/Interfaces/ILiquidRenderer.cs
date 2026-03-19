namespace WeddingApp_Test.Application.Interfaces;

/// <summary>
/// Renders a Liquid template string against a data context dictionary.
/// Used by EmailDispatchService and EmailSchedulerService to produce HTML and subjects.
/// </summary>
public interface ILiquidRenderer
{
    Task<string> RenderAsync(string template, Dictionary<string, object?> context, CancellationToken ct = default);
}

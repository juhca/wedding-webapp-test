namespace WeddingApp_Test.Application.Interfaces.Email;

public interface ILiquidRenderer
{
    /// <summary>
    /// Renders a Liquid template string with the given model dictionary.
    /// </summary>
    Task<string> RenderAsync(string template, Dictionary<string, object?> model);
}
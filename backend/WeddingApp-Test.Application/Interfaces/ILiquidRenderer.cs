namespace WeddingApp_Test.Application.Interfaces;

public interface ILiquidRenderer
{
    /// <summary>
    /// Renders a Liquid template string with the given model.
    /// Property names in templates match C# PascalCase property names (e.g. {{ user.FirstName }}).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the template cannot be parsed.</exception>
    Task<string> RenderAsync(string template, Dictionary<string, object?> model);
}

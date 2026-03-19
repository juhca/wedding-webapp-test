using Fluid;
using Fluid.Values;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.Infrastructure.Email;

/// <summary>
/// Renders Liquid templates using the Fluid library.
/// Supports all standard Liquid syntax: {{ variable }}, {% if %}, {% for %}, filters, etc.
/// </summary>
public class LiquidRenderer : ILiquidRenderer
{
    private static readonly FluidParser Parser = new();

    public Task<string> RenderAsync(string template, Dictionary<string, object?> context, CancellationToken ct = default)
    {
        if (!Parser.TryParse(template, out var fluidTemplate, out var error))
            throw new InvalidOperationException($"Liquid template parse error: {error}");

        var liquidContext = new TemplateContext();

        foreach (var (key, value) in context)
        {
            if (value is not null)
                liquidContext.SetValue(key, FluidValue.Create(value, liquidContext.Options));
        }

        var rendered = fluidTemplate.Render(liquidContext);
        return Task.FromResult(rendered);
    }
}

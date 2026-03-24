using Fluid;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.Infrastructure.Email;

public class LiquidRenderer : ILiquidRenderer
{
    private static readonly FluidParser Parser = new();
    private static readonly TemplateOptions Options = new()
    {
        MemberAccessStrategy = UnsafeMemberAccessStrategy.Instance
    };

    public Task<string> RenderAsync(string template, Dictionary<string, object?> model)
    {
        if (!Parser.TryParse(template, out var fluidTemplate, out var error))
            throw new InvalidOperationException($"Invalid Liquid template: {error}");

        var context = new TemplateContext(Options);
        foreach (var (key, value) in model)
            context.SetValue(key, value);

        return fluidTemplate.RenderAsync(context).AsTask();
    }
}

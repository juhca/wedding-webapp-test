using Fluid;
using WeddingApp_Test.Application.Interfaces.Email;

namespace WeddingApp_Test.Infrastructure.Email;

public class LiquidRenderer : ILiquidRenderer
{
    // FluidParser is thread-safe and expensive to create — make it static
    private static readonly FluidParser Parser = new();
    
    public Task<string> RenderAsync(string template, Dictionary<string, object?> model)
    {
        if (!Parser.TryParse(template, out var liquidTemplate, out var error))
        {
            throw new InvalidOperationException($"Liquid template parse error: {error}");
        }
        
        // TemplateContext holds the variables available during rendering
        var context = new TemplateContext(new TemplateOptions
        {
            // UnsafeMemberAccess lets Fluid read any public C# property
            MemberAccessStrategy = UnsafeMemberAccessStrategy.Instance
        });

        foreach (var (key, value) in model)
        {
            context.SetValue(key, value);
        }
        
        var rendered = liquidTemplate.Render(context);
        
        return Task.FromResult(rendered);
    }
}
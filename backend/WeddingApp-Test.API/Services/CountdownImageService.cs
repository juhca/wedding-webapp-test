using System.Reflection;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WeddingApp_Test.API.Services;

/// <summary>
/// Generates an animated countdown GIF and a personalized guest message PNG.
/// Both are generated on-the-fly using ImageSharp + embedded Roboto font.
/// </summary>
public class CountdownImageService
{
    private static readonly Font _largeFont;
    private static readonly Font _labelFont;
    private static readonly Font _guestFont;

    private const int Width  = 400;
    private const int Height = 100;
    private const int Frames = 60; // 60 seconds of animation

    // Brand colours
    private static readonly Color BgColor    = Color.ParseHex("1a1a2e");
    private static readonly Color NumberColor = Color.ParseHex("e8c99a");
    private static readonly Color LabelColor  = Color.ParseHex("9e9e9e");

    static CountdownImageService()
    {
        var collection = new FontCollection();
        using var fontStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("WeddingApp_Test.API.Resources.Roboto-Regular.ttf")
            ?? throw new InvalidOperationException("Embedded font Roboto-Regular.ttf not found.");

        var family = collection.Add(fontStream);
        _largeFont = family.CreateFont(28, FontStyle.Regular);
        _labelFont = family.CreateFont(11, FontStyle.Regular);
        _guestFont = family.CreateFont(20, FontStyle.Regular);
    }

    /// <summary>
    /// Produces an animated GIF that counts down to <paramref name="targetUtc"/>.
    /// Each of the 60 frames shows the remaining days / hours / min / sec at that moment.
    /// </summary>
    public byte[] GenerateCountdownGif(DateTime targetUtc)
    {
        var now = DateTime.UtcNow;

        using var gif = new Image<Rgba32>(Width, Height);
        var gifMetadata = gif.Metadata.GetGifMetadata();
        gifMetadata.RepeatCount = 0; // loop forever

        for (int frame = 0; frame < Frames; frame++)
        {
            var remaining = targetUtc - now.AddSeconds(frame);
            if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;

            var days    = (int)remaining.TotalDays;
            var hours   = remaining.Hours;
            var minutes = remaining.Minutes;
            var seconds = remaining.Seconds;

            using var frameBitmap = new Image<Rgba32>(Width, Height);
            frameBitmap.Mutate(ctx =>
            {
                ctx.Fill(BgColor);
                DrawUnit(ctx, days,    "DAYS",  40);
                DrawUnit(ctx, hours,   "HOURS", 130);
                DrawUnit(ctx, minutes, "MIN",   225);
                DrawUnit(ctx, seconds, "SEC",   315);
            });

            var frameMetadata = frameBitmap.Frames.RootFrame.Metadata.GetGifMetadata();
            frameMetadata.FrameDelay = 100; // 1 second (in centiseconds)
            frameMetadata.DisposalMethod = GifDisposalMethod.RestoreToBackground;

            gif.Frames.AddFrame(frameBitmap.Frames.RootFrame);
        }

        // Remove the default blank first frame added by ImageSharp
        gif.Frames.RemoveFrame(0);

        using var ms = new MemoryStream();
        gif.SaveAsGif(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Produces a static PNG with a personalised message: "Hi {name}, see you in {days} days!".
    /// </summary>
    public byte[] GenerateGuestMessagePng(string guestFirstName, DateTime targetUtc)
    {
        var days = Math.Max(0, (int)(targetUtc.Date - DateTime.UtcNow.Date).TotalDays);
        var text = $"Hi {guestFirstName}, see you in {days} {(days == 1 ? "day" : "days")}!";

        using var image = new Image<Rgba32>(Width, 60);
        image.Mutate(ctx =>
        {
            ctx.Fill(BgColor);
            var textOptions = new RichTextOptions(_guestFont)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center,
                Origin = new System.Numerics.Vector2(Width / 2f, 30f)
            };
            ctx.DrawText(textOptions, text, NumberColor);
        });

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }

    // ─── Helpers ──────────────────────────────────────────────────────

    private static void DrawUnit(IImageProcessingContext ctx, int value, string label, float centerX)
    {
        var numOptions = new RichTextOptions(_largeFont)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Top,
            Origin = new System.Numerics.Vector2(centerX, 15f)
        };
        ctx.DrawText(numOptions, value.ToString("D2"), NumberColor);

        var labelOptions = new RichTextOptions(_labelFont)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Top,
            Origin = new System.Numerics.Vector2(centerX, 60f)
        };
        ctx.DrawText(labelOptions, label, LabelColor);
    }
}

using WeddingApp_Test.Infrastructure.Email;
using static System.Linq.AsyncEnumerable;

namespace WeddingApp_Test.API.Tests.Email;

/// <summary>
/// Unit tests for <see cref="EmailEventChannel"/>.
/// Verifies that GUID-based email events are published and consumed correctly via the channel.
/// </summary>
[Trait("Category", "EmailEventChannel Unit Tests")]
public class EmailEventChannelTests
{
    /// <summary>
    /// Verifies that a single published GUID can be read back from the channel
    /// and matches the original value.
    /// </summary>
    [Fact]
    public async Task Publish_ThenRead_ReturnsSameId()
    {
        // Arrange
        var channel = new EmailEventChannel();
        var id = Guid.NewGuid();
        
        // Act – publish a single event then read it back with a 1s timeout to avoid hanging
        await channel.PublishAsync(id, CancellationToken.None);
        
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var result = await channel.ReadAllAsync(cts.Token).FirstAsync(cancellationToken: cts.Token);
        
        Assert.Equal(id, result);
    }

    /// <summary>
    /// Verifies that multiple published GUIDs are consumed in the same order
    /// they were published (FIFO behaviour).
    /// </summary>
    [Fact]
    public async Task Publish_Multiple_ReadsInOrder()
    {
        // Arrange
        var channel = new EmailEventChannel();
        var ids = new []{Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};
        
        // Act – publish all events before reading to ensure the channel buffers correctly
        foreach (var id in ids)
        {
            await channel.PublishAsync(id, CancellationToken.None);
        }
        
        // Read until we have collected exactly as many results as were published,
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var results = new List<Guid>();
        await foreach (var id in channel.ReadAllAsync(cts.Token))
        {
            results.Add(id);
            if (results.Count == ids.Length)
            {
                break;
            }
        }
        
        // Assert – order must be preserved (FIFO)
        Assert.Equal(ids, results);
    }
}
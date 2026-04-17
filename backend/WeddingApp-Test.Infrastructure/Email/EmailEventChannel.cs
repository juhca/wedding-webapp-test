using System.Runtime.CompilerServices;
using System.Threading.Channels;
using WeddingApp_Test.Application.Interfaces.Email;

namespace WeddingApp_Test.Infrastructure.Email;

public class EmailEventChannel : IEmailEventChannel
{
    private readonly Channel<Guid> _channel = Channel.CreateBounded<Guid>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader =  true,
            SingleWriter = false,
        });
        
    public async ValueTask PublishAsync(Guid outboxId, CancellationToken ct = default)
    {
        await _channel.Writer.WriteAsync(outboxId, ct);
    }

    public async IAsyncEnumerable<Guid> ReadAllAsync([EnumeratorCancellation] CancellationToken ct)
    {
        await foreach(var id in _channel.Reader.ReadAllAsync(ct))
        {
            yield return id;
        }
    }
}
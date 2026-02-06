using AsyncLocal.ExecutionContext.Abstractions;
using AsyncLocal_demo.Core.BackgroundProcessing;
using System.Threading.Channels;

namespace AsyncLocal_demo.Infrastructure.BackgroundProcessing;

/// <summary>
/// File d'attente de tâches en arrière-plan thread-safe.
/// </summary>
public sealed class BackgroundTaskQueue<TPayload> : IBackgroundTaskQueue<TPayload>
{
    private readonly Channel<BackgroundWorkItem<TPayload>> _channel;
    private readonly IExecutionContext _context;
    private int _pendingCount;

    public BackgroundTaskQueue(IExecutionContext context, int capacity = 100)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
        _channel = Channel.CreateBounded<BackgroundWorkItem<TPayload>>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
    }

    public int PendingCount => _pendingCount;

    public async ValueTask EnqueueAsync(TPayload payload, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var workItem = BackgroundWorkItem<TPayload>.FromContext(payload, _context);
        await _channel.Writer.WriteAsync(workItem, ct);
        Interlocked.Increment(ref _pendingCount);
    }

    public async ValueTask<IBackgroundWorkItem<TPayload>> DequeueAsync(CancellationToken ct)
    {
        var workItem = await _channel.Reader.ReadAsync(ct);
        Interlocked.Decrement(ref _pendingCount);
        return workItem;
    }

    public async IAsyncEnumerable<IBackgroundWorkItem<TPayload>> ReadAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var workItem in _channel.Reader.ReadAllAsync(ct))
        {
            Interlocked.Decrement(ref _pendingCount);
            yield return workItem;
        }
    }
}
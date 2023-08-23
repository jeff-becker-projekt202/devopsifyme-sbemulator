using Amqp;

namespace ServiceBusEmulator.Memory.Entities.Delivering;

internal sealed class TopicDelivery : IDisposable
{
    private bool _disposed;

    public DateTime Posted { get; }

    public Message Message { get; }

    public IReadOnlyList<Delivery> Subscriptions { get; }

    internal TopicDelivery(Message message, IReadOnlyList<Delivery> subscriptions)
    {
        Posted = DateTime.UtcNow;
        Message = message;
        Subscriptions = subscriptions;
    }

    public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(typeof(TopicDelivery).Name);
        }

        IEnumerable<Task<bool>> subscriptionTasks = Subscriptions
            .Select(s => s.WaitAsync(timeout, cancellationToken));
        bool[] results = await Task.WhenAll(subscriptionTasks).ConfigureAwait(false);
        return results.All(r => r);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (Delivery subscription in Subscriptions)
        {
            (subscription as IDisposable)?.Dispose();
        }
    }
}

using Amqp;
using ServiceBusEmulator.Memory.Entities.Delivering;
using System.Text.RegularExpressions;

namespace ServiceBusEmulator.Memory.Entities;

internal sealed class QueueEntity : IEntity, IDisposable
{
    // The name can contain only letters, numbers, periods, hyphens, underscores, tildes, slashes and backward slashes.
    // The name must start and end with a letter or number.
    // The name must be between 1 and 260 characters long.
    private static readonly Regex RxValidName = new("^[A-Za-z0-9]$|^[A-Za-z0-9][\\w\\.\\-\\/~]{0,258}[A-Za-z0-9]$", RegexOptions.Compiled);

    public static string GuardName(string name)
    {
        if (!RxValidName.IsMatch(name))
        {
            throw new ArgumentException(null, nameof(name));
        }
        return name;
    }

    private bool _disposed;
    private readonly DeliveryQueue _deliveryQueue = new();
    private readonly List<Delivery> _deliveries = new();

    public string Name { get; }

    public IReadOnlyList<IDelivery> Deliveries { get; }

    internal QueueEntity(string name)
    {
        GuardName(name);
        Name = name;
        Deliveries = _deliveries.AsReadOnly();
    }

    public IDelivery Post(Message message)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(typeof(QueueEntity).Name);
        }

        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        Delivery delivery = new(message);
        _deliveryQueue.Enqueue(delivery);
        _deliveries.Add(delivery);
        return delivery;
    }

    public override string ToString()
    {
        return Name;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _deliveryQueue.Dispose();

        foreach (Delivery delivery in _deliveries)
        {
            delivery.Dispose();
        }
    }

    void IEntity.Post(Message message)
    {
        _ = Post(message);
    }

    DeliveryQueue IEntity.DeliveryQueue => _deliveryQueue;
}

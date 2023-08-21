using Amqp;
using ServiceBusEmulator.Memory.Entities.Delivering;
using System.Text.RegularExpressions;

namespace ServiceBusEmulator.Memory.Entities;

internal sealed class TopicEntity :  IEntity, IDisposable
{
    // The name can contain only letters, numbers, periods, hyphens, underscores, tildes and slashes.
    // The name must start and end with a letter or number.
    // The name must be between 1 and 260 characters long.
    private static readonly Regex RxValidName = new("^[A-Za-z0-9]$|^[A-Za-z0-9][\\w\\.\\-\\/~]{0,258}[A-Za-z0-9]$", RegexOptions.Compiled);
    private bool _disposed;
    private readonly List<TopicDelivery> _deliveries = new();

    public string Name { get; }

    public IReadOnlyList<ITopicDelivery> Deliveries { get; }

    public IReadOnlyDictionary<string, IEntity> Subscriptions { get; }


    public static string GuardName(string name)
    {
        if (!RxValidName.IsMatch(name))
        {
            throw new ArgumentException(null, nameof(name));
        }
        return name;
    }
    internal TopicEntity(string name, IEnumerable<IEntity> subscriptions)
    {
        GuardName(name);
        Name = $"/{name}";
        Subscriptions = subscriptions
            .ToDictionary(subscription => $"/{Name}/Subscriptions/{subscription.Name}", StringComparer.OrdinalIgnoreCase)
            .AsReadOnly();
        Deliveries = _deliveries.AsReadOnly();
    }

    public ITopicDelivery Post(Message message)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(typeof(TopicEntity).Name);
        }

        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        TopicDelivery delivery = new(message, PostToSubscriptions(message));
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

        foreach (var subscription in Subscriptions.Values)
        {
            ((QueueEntity)subscription).Dispose();
        }

        foreach (TopicDelivery delivery in _deliveries)
        {
            delivery.Dispose();
        }
    }

    private IReadOnlyList<IDelivery> PostToSubscriptions(Message message)
    {
        return Subscriptions
                    .Values
                    .Select(subscription => ((QueueEntity)subscription).Post(message.Clone()))
                    .ToArray();
    }

    void IEntity.Post(Message message)
    {
        _ = Post(message);
    }

    DeliveryQueue IEntity.DeliveryQueue => null;
}
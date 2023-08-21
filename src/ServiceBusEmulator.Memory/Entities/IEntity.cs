using ServiceBusEmulator.Memory.Entities.Delivering;

namespace ServiceBusEmulator.Memory.Entities;

public interface IEntity
{
    string Name { get; }

    DeliveryQueue DeliveryQueue { get; }

    void Post(Amqp.Message message);
}

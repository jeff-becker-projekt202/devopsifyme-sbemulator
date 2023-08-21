using Amqp;
using Amqp.Listener;

namespace ServiceBusEmulator.Memory.Entities.Delivering;

internal interface IDeliveryQueue
{
    void Enqueue(Delivery delivery);

    Message Dequeue(CancellationToken cancellationToken);

    void Process(MessageContext messageContext);
}

using Amqp;
using Amqp.Listener;

namespace ServiceBusEmulator.Memory.Delivering
{
    internal interface IDeliveryQueue
    {
        void Enqueue(Delivery delivery);

        Message Dequeue(CancellationToken cancellationToken);

        void Process(MessageContext messageContext);
    }
}

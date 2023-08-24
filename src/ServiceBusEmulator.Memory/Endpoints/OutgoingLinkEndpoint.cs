using Amqp;
using Amqp.Framing;
using Amqp.Listener;
using System.Collections.Concurrent;

namespace ServiceBusEmulator.Memory.Endpoints;

internal sealed class OutgoingLinkEndpoint : LinkEndpoint
{
    private readonly ConcurrentQueue<Message> _queue;


    public OutgoingLinkEndpoint(ConcurrentQueue<Message> deliveryQueue)
    {
        _queue = deliveryQueue;
    }

    public override void OnLinkClosed(ListenerLink link, Error error)
    {
       
    }

    public override void OnFlow(FlowContext flowContext)
    {
        int messages = flowContext.Messages;
        while (messages > 0 && _queue.Count > 0)
        {
            if (_queue.TryDequeue(out var message))
            {
                flowContext.Link.SendMessage(message);
            }
            messages--;
        }
    }


    public override void OnDisposition(DispositionContext dispositionContext)
    {
        dispositionContext.Complete();
    }

    public override void OnMessage(MessageContext messageContext)
    {
        throw new NotSupportedException();
    }


}

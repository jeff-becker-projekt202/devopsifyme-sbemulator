using Amqp;
using Amqp.Framing;
using Amqp.Listener;
using System.Collections.Concurrent;

namespace ServiceBusEmulator.Memory.Endpoints;



internal sealed class IncomingLinkEndpoint : LinkEndpoint
{
    private readonly ConcurrentQueue<Message> _queue;

    internal IncomingLinkEndpoint(ConcurrentQueue<Message> queue)
    {
        _queue = queue;
    }

    public override void OnLinkClosed(ListenerLink link, Error error)
    {
        base.OnLinkClosed(link, error);
    }

    public override void OnMessage(MessageContext messageContext)
    {
        _queue.Enqueue(messageContext.Message.Clone());
        messageContext.Complete();
    }

    public override void OnFlow(FlowContext flowContext)
    {
    }

    public override void OnDisposition(DispositionContext dispositionContext)
    {
    }
}
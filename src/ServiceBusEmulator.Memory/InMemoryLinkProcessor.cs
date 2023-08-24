using Amqp;
using Amqp.Framing;
using Amqp.Listener;
using Microsoft.Extensions.Logging;
using ServiceBusEmulator.Abstractions.Security;
using ServiceBusEmulator.Memory.Endpoints;

namespace ServiceBusEmulator.Memory;

internal class InMemoryLinkProcessor : ILinkProcessor
{
    private readonly ISecurityContext _securityContext;
    private readonly ChannelMap _channelMap;

    private readonly ILogger _logger;

    public InMemoryLinkProcessor(ISecurityContext securityContext, ChannelMap channelMap, ILogger<InMemoryLinkProcessor> logger)
    {
        _securityContext = securityContext;
        _channelMap = channelMap;
        _logger = logger;
    }

    public void Process(AttachContext attachContext)
    {
        if (string.IsNullOrEmpty(attachContext.Attach.LinkName))
        {
            attachContext.Complete(new Error(ErrorCode.InvalidField) { Description = "Empty link name not allowed." });
            _logger.LogError($"Could not attach empty link to {GetType().Name}.");
            return;
        }

        if (!_securityContext.IsAuthorized(attachContext.Link.Session.Connection))
        {
            attachContext.Complete(new Error(ErrorCode.UnauthorizedAccess) { Description = "Not authorized." });
            _logger.LogError($"Could not attach unathorized link to {GetType().Name}.");
            return;
        }

        if (attachContext.Link.Role)
        {
            AttachIncomingLink(attachContext, (Target)attachContext.Attach.Target);
        }
        else
        {
            AttachOutgoingLink(attachContext, (Source)attachContext.Attach.Source);
        }
    }

    private void AttachIncomingLink(AttachContext attachContext, Target target)
    {
        var queue = _channelMap.GetIncoming(target.Address);

        if (queue == null)
        {
            attachContext.Complete(new Error(ErrorCode.NotFound) { Description = $"Entity not found \"{target.Address}\"." });
            _logger.LogError($"Could not attach incoming link to non-existing entity '{target.Address}'.");
            return;
        }

        var incomingLinkEndpoint = new IncomingLinkEndpoint(queue);
        attachContext.Complete(incomingLinkEndpoint, 300);
        _logger.LogDebug($"Attached incoming link to entity '{target.Address}'.");
    }

    private void AttachOutgoingLink(AttachContext attachContext, Source source)
    {
        var queue = _channelMap.GetIncoming(source.Address);
        if (queue == null)
        {
            attachContext.Complete(new Error(ErrorCode.NotFound) { Description = $"Entity not found \"{source.Address}\"." });
            _logger.LogError($"Could not attach outgoing link to non-existing entity '{source.Address}'.");
            return;
        }



        var outgoingLinkEndpoint = new OutgoingLinkEndpoint(queue);
        attachContext.Complete(outgoingLinkEndpoint, 0);
        _logger.LogDebug($"Attached outgoing link to queue '{source.Address}'.");
    }
}

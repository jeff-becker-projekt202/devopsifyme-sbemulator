using Amqp.Listener;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServiceBusEmulator.Abstractions.Options;
using ServiceBusEmulator.Memory;
using ServiceBusEmulator.Memory.Entities;
using Amqp.Types;
using Amqp;

namespace ServiceBusEmulator;

public static class Extensions
{
    public static IWebAppBuilder AddServiceBusEmulatorMemoryBackend(this IWebAppBuilder builder)
    {
 

        _ = builder.Services.AddSingleton<ILinkProcessor, InMemoryLinkProcessor>();
        _ = builder.Services.AddSingleton<IEntityLookup, EntityLookup>();
        _ = builder.Services.AddSingleton<IHealthCheck, InMemoryHealthCheck>();
        _ = builder.Services.AddOptions<ServiceBusEmulatorOptions>().BindConfiguration("Emulator:Memory");
        _ = builder.Services.AddHealthChecks().AddCheck<InMemoryHealthCheck>("in-memory");

        return builder;
    }
    private static readonly Symbol XOptSequenceNumber = "x-opt-sequence-number";

    internal static Message Clone(this Message message)
    {
        return message == null
                    ? null
                    : Message.Decode(message.Encode());
    }

    internal static Message AddSequenceNumber(this Message message, long sequence)
    {
        if (message != null)
        {
            message.MessageAnnotations ??= new Amqp.Framing.MessageAnnotations();
            if (message.MessageAnnotations[XOptSequenceNumber] == null)
            {
                message.MessageAnnotations[XOptSequenceNumber] = sequence;
            }
        }
        return message;
    }

}

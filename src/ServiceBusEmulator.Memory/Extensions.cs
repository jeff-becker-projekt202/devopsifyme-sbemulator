using Amqp.Listener;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServiceBusEmulator.Abstractions.Options;
using ServiceBusEmulator.Memory;
using ServiceBusEmulator.Memory.Entities;
using Amqp.Types;
using Amqp;

namespace ServiceBusEmulator
{
    public static class Extensions
    {
        public static IServiceCollection AddServiceBusEmulator(this IServiceCollection services, Action<ServiceBusEmulatorOptions> configure = null)
        {
            configure ??= (o) => { };

            _ = services.AddSingleton<ILinkProcessor, InMemoryLinkProcessor>();
            _ = services.AddSingleton<IEntityLookup, EntityLookup>();
            _ = services.AddSingleton<IHealthCheck, InMemoryHealthCheck>();



            return services;
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
}

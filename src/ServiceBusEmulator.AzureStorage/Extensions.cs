using Microsoft.Extensions.DependencyInjection;
using ServiceBusEmulator.Abstractions.Options;
using Amqp.Types;
using Amqp;
using ServiceBusEmulator.AzureStorage;
using Amqp.Listener;
using Microsoft.Extensions.Hosting;

namespace ServiceBusEmulator;

public static class Extensions
{
    public static IWebAppBuilder AddServiceBusEmulatorAzureStorageBackend(this IWebAppBuilder builder)
    {


        _ = builder.Services.AddSingleton<ILinkProcessor, AzureStorageLinkProcessor>();
        _ = builder.Services.AddSingleton<IHostedService, AzureStorageMessagePump>();
        _ = builder.Services.AddOptions<AzureStorageBackendOptions>().BindConfiguration("Emulator:AzureStorage");
        _ = builder.Services.AddHealthChecks().AddCheck<AzureStorageHealthCheck>("azure-storage");

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
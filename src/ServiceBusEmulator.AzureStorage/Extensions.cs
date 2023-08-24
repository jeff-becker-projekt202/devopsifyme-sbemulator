using Microsoft.Extensions.DependencyInjection;
using ServiceBusEmulator.Abstractions.Options;
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


}
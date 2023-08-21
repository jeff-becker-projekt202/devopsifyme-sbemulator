using Amqp.Listener;
using Microsoft.Extensions.DependencyInjection;
using ServiceBusEmulator.Abstractions.Options;
using ServiceBusEmulator.RabbitMq.Commands;
using ServiceBusEmulator.RabbitMq.Endpoints;
using ServiceBusEmulator.RabbitMq.Links;
using ServiceBusEmulator.RabbitMq.Options;

namespace ServiceBusEmulator.RabbitMq
{
    public static class Extensions
    {
        public static IWebAppBuilder AddServiceBusEmulatorRabbitMqBackend(this IWebAppBuilder builder, Action<RabbitMqBackendOptions>? configure = null)
        {
            configure ??= (o) => { };

            _ = builder.Services.AddSingleton<ILinkProcessor, RabbitMqLinkProcessor>();
            _ = builder.Services.AddSingleton<IRabbitMqLinkRegister, RabbitMqLinkRegister>();
            _ = builder.Services.AddSingleton<IRabbitMqChannelFactory, RabbitMqChannelFactory>();
            _ = builder.Services.AddTransient<IRabbitMqLinkEndpointFactory, RabbitMqLinkEndpointFactory>();
            _ = builder.Services.AddTransient<IRabbitMqManagementCommandFactory, RabbitMqManagementCommandFactory>();
            _ = builder.Services.AddTransient<IRabbitMqDeliveryTagTracker, RabbitMqDeliveryTagTracker>();
            _ = builder.Services.AddTransient<IRabbitMqInitializer, RabbitMqInitializer>();

            _ = builder.Services.AddTransient<IRabbitMqUtilities, RabbitMqUtilities>();
            _ = builder.Services.AddTransient<IRabbitMqMapper, RabbitMqMapper>();
            _ = builder.Services.AddTransient<RabbitMqSenderEndpoint>();
            _ = builder.Services.AddTransient<RabbitMqReceiverEndpoint>();
            _ = builder.Services.AddTransient<RabbitMqManagementSenderEndpoint>();
            _ = builder.Services.AddTransient<RabbitMqManagementReceiverEndpoint>();

            _ = builder.Services.AddTransient<RenewLockCommand>();
            _ = builder.Services.AddTransient<PeekMessageCommand>();
            _ = builder.Services.AddTransient<RenewSessionLockCommand>();
            _ = builder.Services.AddTransient<SetSessionStateCommand>();
            _ = builder.Services.AddTransient<GetSessionStateCommand>();

            _ = builder.Services.AddOptions<RabbitMqBackendOptions>().Configure(configure).BindConfiguration("Emulator:RabbitMq");

            _ = builder.Services.AddHealthChecks().AddCheck<RabbitMqHealthCheck>("rabbitmq");

            return builder;
        }
    }
}

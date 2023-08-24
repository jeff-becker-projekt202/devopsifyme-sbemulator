using Amqp.Listener;
using ServiceBusEmulator.Abstractions;
using ServiceBusEmulator.Abstractions.Options;
using ServiceBusEmulator.RabbitMq.Commands;
using ServiceBusEmulator.RabbitMq.Endpoints;
using ServiceBusEmulator.RabbitMq.Links;
using ServiceBusEmulator.RabbitMq.Options;
using Microsoft.Extensions.DependencyInjection;
using ServiceBusEmulator.Abstractions.Configuration;

namespace ServiceBusEmulator.RabbitMq;
public class RabbitMqBackend : IBackend
{
    private const string ConfigSectionPath = "Emulator:RabbitMq";

    public string Name => "RabbitMq";
    public void ApplyConfiguration(IWebAppBuilder builder)
    {
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

        _ = builder.Services.AddOptions<RabbitMqBackendOptions>().BindConfiguration(ConfigSectionPath);

        _ = builder.Services.AddHealthChecks().AddCheck<RabbitMqHealthCheck>("rabbitmq");
    }
    private readonly SwitchMapBuilder<RabbitMqBackendOptions> _swtichMap =
        SwitchMapBuilder<RabbitMqBackendOptions>.Create(ConfigSectionPath)
            .Add("rabbitmq-channel", x=>x.Channels)
            .Add("rabbitmq-user", x=>x.Username)
            .Add("rabbitmq-password",x=>x.Password)
            .Add("rabbitmq-host", x=>x.Host)
            .Add("rabbitmq-port", x => x.Port);
    public IMapSwitches SwitchMappings => _swtichMap.Mapper;
}

using ServiceBusEmulator.Abstractions.Options;

namespace ServiceBusEmulator.Host;

public class AppBuilderWrapper : IWebAppBuilder
{
    private WebApplicationBuilder _builder;

    public AppBuilderWrapper(WebApplicationBuilder builder)
    {
        _builder = builder;
    }

    public IHostEnvironment Environment => _builder.Environment;

    public IServiceCollection Services => _builder.Services;

    public IConfigurationBuilder ConfigurationBuilder => _builder.Configuration;
    public IConfiguration Configuration =>_builder.Configuration;

    public ILoggingBuilder Logging => _builder.Logging;

    public WebApplication Build() => _builder.Build();

    public string DebugConfig => ((IConfigurationRoot)_builder.Configuration).GetDebugView();
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace ServiceBusEmulator.Abstractions.Options;
public interface IWebAppBuilder
{
    /// <summary>
    /// Provides information about the web hosting environment an application is running.
    /// </summary>
    IHostEnvironment Environment { get; }
    IServiceCollection Services { get; }
    IConfigurationBuilder ConfigurationBuilder { get; }
    IConfiguration Configuration { get; }
    ILoggingBuilder Logging { get; }
}

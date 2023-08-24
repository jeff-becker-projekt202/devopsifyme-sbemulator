using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ServiceBusEmulator.AzureStorage;
public class AzureStorageMessagePump : BackgroundService
{
    private readonly ILogger<AzureStorageMessagePump> _logger;

    public AzureStorageMessagePump( ILogger<AzureStorageMessagePump> logger)
    {
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

    }
}

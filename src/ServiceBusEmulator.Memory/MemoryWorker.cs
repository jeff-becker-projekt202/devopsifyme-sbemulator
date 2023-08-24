using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ServiceBusEmulator.Memory;
public class MemoryWorker : BackgroundService
{
    private readonly ILogger<MemoryWorker> _logger;


    public MemoryWorker(ILogger<MemoryWorker> logger)
    {
        _logger = logger;

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        _logger.LogInformation("Memory Worker started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        
    }
}
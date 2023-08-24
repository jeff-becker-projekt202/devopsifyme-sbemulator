using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ServiceBusEmulator.AzureStorage;
public class AzureStorageHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) => Task.FromResult(HealthCheckResult.Healthy());
}

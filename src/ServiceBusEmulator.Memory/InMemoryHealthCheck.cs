using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ServiceBusEmulator.Memory;
internal class InMemoryHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) => Task.FromResult(HealthCheckResult.Healthy());
}

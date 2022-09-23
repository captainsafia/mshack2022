
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebSample.HealthChecks
{
    public class GetValuesHealthCheck : IHealthCheck
    {
        public GetValuesHealthCheck()
        {
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

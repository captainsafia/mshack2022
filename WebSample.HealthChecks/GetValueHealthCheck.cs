using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebSample.Shared.Processors;

namespace WebSample.HealthChecks
{
    public class GetValuesHealthCheck : IHealthCheck
    {
        private readonly IValueProcessor _processor;

        public GetValuesHealthCheck(IValueProcessor processor)
        {
            _processor = processor;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            _ = await _processor.GetValues();

            return HealthCheckResult.Healthy();
        }
    }
}

using Fcg.Core.Abstractions.Common;
using Fcg.Payments.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fcg.Payments.Infrastructure.Extensions
{
    internal static class ObservabilityExtension
    {
        public static IServiceCollection AddHealthCheckExtension(this IServiceCollection services)
        {
            services.AddHealthChecks()
               .AddCheck("live", () => HealthCheckResult.Healthy(), tags: new[] { HealthCheckTags.Live })
               .AddDbContextCheck<PaymentDbContext>(
               name: "database-healthcheck",
               tags: new[] { HealthCheckTags.Ready});
            return services;
        }
    }
}

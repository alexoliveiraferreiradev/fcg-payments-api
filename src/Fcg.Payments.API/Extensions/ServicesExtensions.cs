using Fcg.Payments.Application.Extensions;
using Fcg.Payments.Infrastructure.Extensions;

namespace Fcg.Payments.API.Extensions
{
    public static class ServicesExtensions
    {
        public static WebApplicationBuilder AddServicesExtension(this WebApplicationBuilder builder)
        {
            builder.AddObservabilityExtension();
            builder.Services.AddAplicationServices();
            builder.Services.AddInfrastructure(builder.Configuration);
            return builder;
        }       
    }
}

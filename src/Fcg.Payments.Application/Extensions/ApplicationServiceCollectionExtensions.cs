using Fcg.Payments.Application.Features.Commands.ProcessPayment;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Payments.Application.Extensions
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddAplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(ProcessPaymentCommand).Assembly);
            });

            return services;
        }
    }
}

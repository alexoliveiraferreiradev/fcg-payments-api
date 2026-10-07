using Fcg.Core.Abstractions.Interfaces;
using Fcg.Payments.Domain.Repositories;
using Fcg.Payments.Domain.Services;
using Fcg.Payments.Infrastructure.Persistence;
using Fcg.Payments.Infrastructure.Repository;
using Fcg.Payments.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Fcg.Payments.Infrastructure.Extensions
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDatabaseExtension(configuration);
            services.AddMassTransitExtension(configuration);  
            services.AddHealthCheckExtension();
            services.AddScoped<IDbConnection>(sp => sp.GetRequiredService<PaymentDbContext>().Database.GetDbConnection());
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPaymentSimulator, PaymentSimulator>();
            return services;
        }
    }
}

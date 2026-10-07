using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.WebApi.MessageBroker;
using Fcg.Payments.Infrastructure.Consumers;
using Fcg.Payments.Infrastructure.MessageBroker;
using Fcg.Payments.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fcg.Payments.Infrastructure.Extensions
{
    internal static class MessageBrokerExtensions
    {
        public static IServiceCollection AddMassTransitExtension(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();

            services.AddOptions<PaymentServiceRabbitMqSettings>().BindConfiguration(PaymentServiceRabbitMqSettings.SectionName)
            .ValidateDataAnnotations().ValidateOnStart();

            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(DeliveryFailedEventConsumer).Assembly);
                x.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });


                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqConfig = context.GetRequiredService<IOptions<PaymentServiceRabbitMqSettings>>().Value;

                    cfg.Host(rabbitMqConfig.Host, rabbitMqConfig.Port, "/", h =>
                    {
                        h.Username(rabbitMqConfig.Username);
                        h.Password(rabbitMqConfig.Password);
                    });

                    cfg.UseMessageRetry(r =>
                    {
                        r.Interval(3, TimeSpan.FromSeconds(5));
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.PaymentOrderQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<PaymentDbContext>(context);
                        e.ConfigureConsumer<OrderPlacedEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.PaymentDeliveryFailedQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<PaymentDbContext>(context);
                        e.ConfigureConsumer<DeliveryFailedEventConsumer>(context);
                    });


                });
            });
            return services;
        }
    }
}

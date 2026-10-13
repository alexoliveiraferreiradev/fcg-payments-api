using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
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

            services.AddOptions<AzureServiceBusSettings>().BindConfiguration(AzureServiceBusSettings.SectionName)
            .ValidateDataAnnotations().ValidateOnStart();

            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(DeliveryFailedEventConsumer).Assembly);
                x.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    var serviceBusConfig = context.GetRequiredService<IOptions<AzureServiceBusSettings>>().Value;
                    cfg.Host(serviceBusConfig.ConnectionString);
                    cfg.UseMessageRetry(r =>
                    {
                        r.Interval(3, TimeSpan.FromSeconds(5));
                    });

                    cfg.Message<IPaymentApprovedIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.PaymentApproved));
                    cfg.Message<IPurchaseRejectedIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.PurchaseRejected));
                    cfg.Message<IDeliveryFailedIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.DeliveryFailed));
                    cfg.Message<IOrderPlacedIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.OrderPlaced));

                    cfg.ReceiveEndpoint(serviceBusConfig.PaymentOrderQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<PaymentDbContext>(context);
                        e.ConfigureConsumer<OrderPlacedEventConsumer>(context);
                    });
                    cfg.ReceiveEndpoint(serviceBusConfig.PaymentDeliveryFailedQueue, e =>
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

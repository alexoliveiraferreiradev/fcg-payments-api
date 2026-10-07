using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Payments.Application.Features.Commands.ProcessPayment;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Payments.Infrastructure.Consumers
{
    public class OrderPlacedEventConsumer : IConsumer<IOrderPlacedIntegrationEvent>
    {
        private readonly ISender _sender;
        private readonly ILogger<OrderPlacedEventConsumer> _logger;        
        public OrderPlacedEventConsumer(ILogger<OrderPlacedEventConsumer> logger,
            ISender sender)
        {
            _logger = logger;
            _sender = sender;
        }
        public async Task Consume(ConsumeContext<IOrderPlacedIntegrationEvent> context)
        {
            var pedido = context.Message;
            _logger.LogInformation("[PaymentsAPI] Processando Pagamento para o pedido {OrderId} do usuário {UserId} no valor de R$ {Price}",
               pedido.OrderId, pedido.UserId, pedido.AmountPrice);

            var processCommand = new ProcessPaymentCommand(
                    pedido.EventId,
                    pedido.CreatedAt,
                    pedido.UserId,
                    pedido.OrderId,
                    pedido.AmountPrice,
                    pedido.GameIds
            );

            await _sender.Send(processCommand);
        }
    }
}

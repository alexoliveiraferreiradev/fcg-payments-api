using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Payments.Application.Features.Commands.RefundPayment;
using MassTransit;
using MediatR;

namespace Fcg.Payments.Infrastructure.Consumers
{
    public class DeliveryFailedEventConsumer : IConsumer<IDeliveryFailedIntegrationEvent>
    {
        private readonly ISender _sender;
        public DeliveryFailedEventConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<IDeliveryFailedIntegrationEvent> context)
        {
            var message = context.Message;
            var refundCommand = new RefundPaymentCommand(
                                message.EventId,
                                message.CreatedAt,
                                message.UserId, 
                                message.OrderId);
            await _sender.Send(refundCommand);
        }
    }
}



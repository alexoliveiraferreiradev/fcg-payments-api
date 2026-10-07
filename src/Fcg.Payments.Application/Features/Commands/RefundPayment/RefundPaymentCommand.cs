using MediatR;

namespace Fcg.Payments.Application.Features.Commands.RefundPayment
{
    public record RefundPaymentCommand(Guid EventId, DateTime CreatedAt,Guid UserId, Guid OrderId) : IRequest;
}

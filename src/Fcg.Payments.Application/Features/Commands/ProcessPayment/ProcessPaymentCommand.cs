using Fcg.Payments.Domain.Enum;
using MediatR;

namespace Fcg.Payments.Application.Features.Commands.ProcessPayment
{
    public record ProcessPaymentCommand(Guid EventId, DateTime CreatedAt,Guid UserId,Guid OrderId,decimal Price, IEnumerable<Guid> GameIds) : IRequest;
}

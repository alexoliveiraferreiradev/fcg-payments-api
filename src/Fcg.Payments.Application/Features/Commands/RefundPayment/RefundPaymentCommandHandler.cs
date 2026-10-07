using Fcg.Core.Abstractions.Interfaces;
using Fcg.Payments.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Payments.Application.Features.Commands.RefundPayment
{
    public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<RefundPaymentCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public RefundPaymentCommandHandler(
            IPaymentRepository paymentRepository,
            ILogger<RefundPaymentCommandHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var payment = await _paymentRepository.GetPaymentsByOrderId(request.OrderId);
                if (payment == null)
                {
                    _logger.LogWarning("[PaymentsAPI] Pagamento não encontrado para o pedido {OrderId}", request.OrderId);
                    return;
                }
                payment.RefundPayment();

                await _unitOfWork.CommitAsync();
                _logger.LogInformation("[PaymentsAPI] Pagamento estornado com sucesso para o pedido {OrderId}", request.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PaymentsAPI] Erro ao estornar Pagamento para o pedido {OrderId}", request.OrderId);
                throw;
            }
        }
    }
}

using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Payments.Domain.Entities;
using Fcg.Payments.Domain.Repositories;
using Fcg.Payments.Domain.Services;
using Fcg.Payments.Domain.ValueObject;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Payments.Application.Features.Commands.ProcessPayment
{
    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<ProcessPaymentCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;
        private readonly IPaymentSimulator _paymentSimulator;

        public ProcessPaymentCommandHandler(
            IPaymentRepository paymentRepository,
            ILogger<ProcessPaymentCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IIntegrationEventPublisher integrationEventPublisher,
            IPaymentSimulator paymentSimulator)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _integrationEventPublisher = integrationEventPublisher;
            _paymentSimulator = paymentSimulator;
        }

        public async Task Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var priceValueObject = new Price(request.Price);

                _logger.LogInformation("[PaymentsAPI] Registrando transação de Pagamento. Pedido: {OrderId}, Valor: R${Price}", request.OrderId, request.Price);

                bool paymentApproved = await _paymentSimulator.SimulateProcessing(request.Price);
                var paymentMethod = await _paymentSimulator.SimulatePaymentMethod();

                if (paymentApproved)
                {
                    _logger.LogInformation("[PaymentsAPI] Pagamento APROVADO para o pedido {OrderId}", request.OrderId);

                    var payment = new Payment(request.OrderId, request.UserId, priceValueObject);

                    payment.ApprovePayment();

                    _paymentRepository.AddPayment(payment);                 

                    await _integrationEventPublisher.PublishAsync<IPaymentProcessedIntegrationEvent>(new
                    {
                        EventId = request.EventId,
                        CreatedAt = request.CreatedAt,
                        OrderId = request.OrderId,
                        UserId = request.UserId,
                        GameIds = request.GameIds
                    },cancellationToken);
                    
                }
                else
                {
                    _logger.LogWarning("[PaymentsAPI] Pagamento REJEITADO para o pedido {OrderId}", request.OrderId);
                    var errorReason = "Pagamento recusado na simulação.";

                    var payment = new Payment(request.OrderId, request.UserId, priceValueObject);

                    payment.RejectPayment();

                    _paymentRepository.AddPayment(payment);

                    await _integrationEventPublisher.PublishAsync<IPaymentFailedIntegrationEvent>(new
                    {
                        OrderId = request.OrderId,
                        UserId = request.UserId,
                        Reason = errorReason
                    }, cancellationToken);
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PaymentsAPI] Erro crítico ao processar o pagamento do pedido {OrderId}", request.OrderId);
            }
        }
    }
}

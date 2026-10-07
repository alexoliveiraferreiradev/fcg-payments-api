using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Payments.Application.Features.Commands.ProcessPayment;
using Fcg.Payments.Domain.Entities;
using Fcg.Payments.Domain.Enum;
using Fcg.Payments.Domain.Repositories;
using Fcg.Payments.Domain.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fcg.Payments.Application.Tests.Features.Commands.ProcessPayment
{
    public class ProcessPaymentCommandHandlerTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<ILogger<ProcessPaymentCommandHandler>> _loggerMock;
        private readonly Mock<IIntegrationEventPublisher> _integrationEventPublisherMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPaymentSimulator> _paymentSimulatorMock;
        private readonly ProcessPaymentCommandHandler _handler;

        public ProcessPaymentCommandHandlerTests()
        {
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _loggerMock = new Mock<ILogger<ProcessPaymentCommandHandler>>();
            _integrationEventPublisherMock = new Mock<IIntegrationEventPublisher>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _paymentSimulatorMock = new Mock<IPaymentSimulator>();

            _paymentSimulatorMock
                .Setup(s => s.SimulatePaymentMethod())
                .ReturnsAsync(PaymentMethod.CartaoCredito);

            _handler = new ProcessPaymentCommandHandler(
                _paymentRepositoryMock.Object,
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _integrationEventPublisherMock.Object,
                _paymentSimulatorMock.Object
            );
        }

        private static ProcessPaymentCommand CreateCommand(decimal price = 100.50m) =>
            new(
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid(),
                Guid.NewGuid(),
                price,
                new List<Guid> { Guid.NewGuid() }
            );

        [Fact]
        public async Task Handle_ShouldPassAndApprovePayment_WhenSimulationApprovesTransaction()
        {
            // Arrange
            var command = CreateCommand();

            _paymentSimulatorMock
                .Setup(s => s.SimulateProcessing(command.Price))
                .ReturnsAsync(true);

            Payment? capturedPayment = null;
            _paymentRepositoryMock
                .Setup(r => r.AddPayment(It.IsAny<Payment>()))
                .Callback<Payment>(p => capturedPayment = p);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            capturedPayment.Should().NotBeNull();
            capturedPayment!.Status.Should().Be(StatusPayment.Paid);
            _paymentRepositoryMock.Verify(r => r.AddPayment(It.IsAny<Payment>()), Times.Once);
            _integrationEventPublisherMock.Verify(
                p => p.PublishAsync<IPaymentProcessedIntegrationEvent>(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _integrationEventPublisherMock.Verify(
                p => p.PublishAsync<IPaymentFailedIntegrationEvent>(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldPassAndRejectPayment_WhenSimulationRejectsTransaction()
        {
            // Arrange
            var command = CreateCommand();

            _paymentSimulatorMock
                .Setup(s => s.SimulateProcessing(command.Price))
                .ReturnsAsync(false);

            Payment? capturedPayment = null;
            _paymentRepositoryMock
                .Setup(r => r.AddPayment(It.IsAny<Payment>()))
                .Callback<Payment>(p => capturedPayment = p);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            capturedPayment.Should().NotBeNull();
            capturedPayment!.Status.Should().Be(StatusPayment.Rejected);
            _paymentRepositoryMock.Verify(r => r.AddPayment(It.IsAny<Payment>()), Times.Once);
            _integrationEventPublisherMock.Verify(
                p => p.PublishAsync<IPaymentFailedIntegrationEvent>(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _integrationEventPublisherMock.Verify(
                p => p.PublishAsync<IPaymentProcessedIntegrationEvent>(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotThrow_WhenCriticalErrorOccurs()
        {
            // Arrange
            var command = CreateCommand(50.0m);

            _paymentSimulatorMock
                .Setup(s => s.SimulateProcessing(command.Price))
                .ThrowsAsync(new Exception("Erro de conexão com o banco"));

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            _paymentRepositoryMock.Verify(r => r.AddPayment(It.IsAny<Payment>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}

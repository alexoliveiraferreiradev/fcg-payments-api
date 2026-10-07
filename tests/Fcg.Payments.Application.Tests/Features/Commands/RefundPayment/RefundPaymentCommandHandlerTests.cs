using Fcg.Core.Abstractions.Interfaces;
using Fcg.Payments.Application.Features.Commands.RefundPayment;
using Fcg.Payments.Domain.Entities;
using Fcg.Payments.Domain.Enum;
using Fcg.Payments.Domain.Repositories;
using Fcg.Payments.Domain.ValueObject;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fcg.Payments.Application.Tests.Features.Commands.RefundPayment
{
    public class RefundPaymentCommandHandlerTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<ILogger<RefundPaymentCommandHandler>> _loggerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly RefundPaymentCommandHandler _handler;

        public RefundPaymentCommandHandlerTests()
        {
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _loggerMock = new Mock<ILogger<RefundPaymentCommandHandler>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new RefundPaymentCommandHandler(
                _paymentRepositoryMock.Object,
                _loggerMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldPassAndCommit_WhenPaymentExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var command = new RefundPaymentCommand(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), orderId);
            var payment = new Payment(orderId, command.UserId, new Price(100m));

            _paymentRepositoryMock
                .Setup(r => r.GetPaymentsByOrderId(orderId))
                .ReturnsAsync(payment);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            payment.Status.Should().Be(StatusPayment.Refunded);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotThrowNorCommit_WhenPaymentNotFound()
        {
            // Arrange
            var command = new RefundPaymentCommand(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid());

            _paymentRepositoryMock
                .Setup(r => r.GetPaymentsByOrderId(command.OrderId))
                .ReturnsAsync((Payment?)null);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenRepositoryThrowsException()
        {
            // Arrange
            var command = new RefundPaymentCommand(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid());

            _paymentRepositoryMock
                .Setup(r => r.GetPaymentsByOrderId(command.OrderId))
                .ThrowsAsync(new Exception("Erro de conexão com o banco"));

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>();
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenCommitThrowsException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var command = new RefundPaymentCommand(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), orderId);
            var payment = new Payment(orderId, command.UserId, new Price(75m));

            _paymentRepositoryMock
                .Setup(r => r.GetPaymentsByOrderId(orderId))
                .ReturnsAsync(payment);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync())
                .ThrowsAsync(new Exception("Falha ao persistir"));

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>();
            payment.Status.Should().Be(StatusPayment.Refunded);
        }
    }
}

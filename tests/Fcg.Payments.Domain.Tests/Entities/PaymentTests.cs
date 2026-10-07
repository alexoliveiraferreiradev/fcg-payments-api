using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Payments.Domain.Entities;
using Fcg.Payments.Domain.Enum;
using Fcg.Payments.Domain.ValueObject;
using FluentAssertions;
using System;
using Xunit;

namespace Fcg.Payments.Domain.Tests.Entities
{
    public class PaymentTests
    {
        [Fact]
        public void Construtor_DeveCriarPaymentValido_E_ComStatusPendente()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var UserId = Guid.NewGuid();
            var value = new Price(100.50m);

            // Act
            var Payment = new Payment(orderId, UserId, value);

            // Assert
            Payment.OrderId.Should().Be(orderId);
            Payment.UserId.Should().Be(UserId);
            Payment.Amount.Value.Should().Be(100.50m);
            Payment.Status.Should().Be(StatusPayment.Pending);
            Payment.Transactions.Should().BeEmpty();
        }

        [Fact]
        public void Construtor_DeveLancarDomainException_QuandoPrecoForNegativo()
        {
            // Arrange & Act
            Action act = () => new Payment(Guid.NewGuid(), Guid.NewGuid(), new Price(-10.0m));

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void RegistrarPayment_DeveMudarStatusParaPago_QuandoTransacaoForAprovada()
        {
            // Arrange
            var Payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), new Price(50.0m));
            var transaction = new Transaction(Payment.Id, PaymentMethod.CartaoCredito, TransactionStatus.Aprovada, null);

            // Act
            Payment.RegisterTransaction(transaction);

            // Assert
            Payment.Status.Should().Be(StatusPayment.Paid);
            Payment.Transactions.Should().Contain(transaction);
        }

        [Fact]
        public void RegistrarPayment_DeveMudarStatusParaRejeitado_QuandoTransacaoForRecusada()
        {
            // Arrange
            var Payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), new Price(50.0m));
            var transaction = new Transaction(Payment.Id, PaymentMethod.CartaoCredito, TransactionStatus.Recusada, "Saldo Insuficiente");

            // Act
            Payment.RegisterTransaction(transaction);

            // Assert
            Payment.Status.Should().Be(StatusPayment.Rejected);
            Payment.Transactions.Should().Contain(transaction);
        }
    }
}

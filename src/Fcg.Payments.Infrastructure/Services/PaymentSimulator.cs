using Fcg.Payments.Domain.Enum;
using Fcg.Payments.Domain.Services;

namespace Fcg.Payments.Infrastructure.Services
{
    internal class PaymentSimulator : IPaymentSimulator
    {
        public Task<bool> SimulateProcessing(decimal value)
        {
            if (value == 0) return Task.FromResult(true);

            var sucesso = new Random().Next(1, 101) <= 90;
            return Task.FromResult(sucesso);
        }

        public Task<PaymentMethod> SimulatePaymentMethod()
        {
            var paymentMethod = new Random().Next(1, 3);
            return Task.FromResult((PaymentMethod)paymentMethod);
        }
    }
}

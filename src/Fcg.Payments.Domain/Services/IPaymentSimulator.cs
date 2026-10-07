using Fcg.Payments.Domain.Enum;

namespace Fcg.Payments.Domain.Services
{
    public interface IPaymentSimulator
    {
        Task<bool> SimulateProcessing(decimal value);
        Task<PaymentMethod> SimulatePaymentMethod();
    }
}

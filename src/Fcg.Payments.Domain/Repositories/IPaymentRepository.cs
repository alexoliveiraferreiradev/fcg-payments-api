using Fcg.Payments.Domain.Entities;

namespace Fcg.Payments.Domain.Repositories
{
    public interface IPaymentRepository
    {
        void AddPayment(Payment Payment);                
        
        Task<Payment?> GetPaymentsByUserId(Guid UserId);
        Task<Payment?> GetPaymentsByOrderId(Guid OrderId);
    }
}

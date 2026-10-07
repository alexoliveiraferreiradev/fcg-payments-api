using Fcg.Payments.Domain.Entities;
using Fcg.Payments.Domain.Repositories;
using Fcg.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Payments.Infrastructure.Repository
{
    internal class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _dbContext;

        public PaymentRepository(PaymentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddPayment(Payment Payment)
        {
            _dbContext.Payments.Add(Payment);
        }

        public async Task<Payment?> GetPaymentsByOrderId(Guid OrderId)
        {
            return await _dbContext.Payments.FirstOrDefaultAsync(x=>x.OrderId == OrderId);
        }

        public async Task<Payment?> GetPaymentsByUserId(Guid UserId)
        {
            return await _dbContext.Payments.FirstOrDefaultAsync(x=>x.UserId == UserId);
        }

       
    }
}

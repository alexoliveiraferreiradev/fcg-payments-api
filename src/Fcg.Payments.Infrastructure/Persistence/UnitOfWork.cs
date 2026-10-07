using Fcg.Core.Abstractions.Interfaces;

namespace Fcg.Payments.Infrastructure.Persistence
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly PaymentDbContext _dbContext;

        public UnitOfWork(PaymentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> CommitAsync()
        {
            return await _dbContext.SaveChangesAsync() > 0; 
        }
    }
}

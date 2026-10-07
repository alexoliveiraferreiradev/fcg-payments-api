using Fcg.Core.Abstractions.Common;
using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Common.Interfaces;
using Fcg.Core.Abstractions.Resources;
using Fcg.Payments.Domain.Enum;
using Fcg.Payments.Domain.ValueObject;

namespace Fcg.Payments.Domain.Entities
{
    public class Payment : AggregateRoot
    {
        public Guid OrderId { get; private set; }
        public Guid UserId { get; set; }
        public Price Amount { get; set; }
        public StatusPayment Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        
        public virtual ICollection<Transaction> Transactions { get; private set; }
        protected Payment() { } 

        protected override void ValidateEntity()
        {
            if(Amount == null) throw new DomainException(DomainMessages.PriceRequired);
            if(Amount.Value < 0) throw new DomainException(DomainMessages.InvalidValue);           
        }

        public Payment(Guid orderId, Guid userId, Price amount)
        {
            OrderId = orderId;
            UserId = userId;
            Amount = amount;
            Status = StatusPayment.Pending;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Transactions = new List<Transaction>();
        }

        public void RejectPayment()
        {
            Status = StatusPayment.Rejected;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ApprovePayment()
        {
            Status = StatusPayment.Paid;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RefundPayment()
        {
            Status = StatusPayment.Refunded;
            UpdatedAt = DateTime.UtcNow;
        }   

        public void RegisterTransaction(Transaction transaction)
        {
            Transactions.Add(transaction);
            if(transaction.Status == TransactionStatus.Aprovada)
            {
                Status = StatusPayment.Paid;
            }
            else if (transaction.Status == TransactionStatus.Recusada)
            {
                Status = StatusPayment.Rejected;
            }

            UpdatedAt = DateTime.UtcNow;
        }
    }
  
}

using Fcg.Payments.Domain.Enum;

namespace Fcg.Payments.Domain.Entities
{
    public class Transaction 
    {
        public Guid Id { get; private set; }
        public Guid PaymentId { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }
        public TransactionStatus Status { get; private set; }
        public string? ErrorMessage { get; private set; }
        public DateTime AttemptDate { get; private set; }
        public Transaction() { }

        public Transaction(Guid paymentId, PaymentMethod paymentMethod, TransactionStatus status, string? errorMessage)
        {
            Id = Guid.NewGuid();
            PaymentId = paymentId;
            PaymentMethod = paymentMethod;
            Status = status;
            ErrorMessage = errorMessage;
            AttemptDate = DateTime.UtcNow;
        }
    }
}

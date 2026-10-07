using Fcg.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Payments.Infrastructure.Persistence.Mappings
{
    public class TransactionMapping : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .ValueGeneratedNever();

            builder.Property(t => t.PaymentId)
                .IsRequired();

            builder.Property(t => t.PaymentMethod)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(t => t.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(t => t.ErrorMessage)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(t => t.AttemptDate)
                .IsRequired();

            builder.HasIndex(t => t.PaymentId);
        }
    }
}

using Fcg.Core.Abstractions.Common;
using Fcg.Core.Abstractions.Resources;

namespace Fcg.Payments.Domain.ValueObject
{
    public class Price : ValueObject<Price>
    {
        public decimal Value { get; }

        public Price(decimal value)
        {
            AssertionConcern.AssertArgumentValueFormat(value, DomainMessages.InvalidValue);
            Value = value;
        }

        protected override bool EqualsCore(Price other)
        {
            return Value == other.Value;
        }

        protected override int GetHashCodeCore()
        {
            return Value.GetHashCode();
        }
    }
}

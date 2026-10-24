using System.ComponentModel.DataAnnotations;

namespace Fcg.Payments.Infrastructure.MessageBroker
{
    public class AzureServiceBusSettings
    {
        public const string SectionName = "AzureServiceBus";
        [Required]
        public string ConnectionString { get; init; } = string.Empty;       
        [Required(ErrorMessage = "PaymentOrderQueue é obrigatório")]
        public string PaymentOrderQueue { get; set; } = string.Empty;
        [Required(ErrorMessage = "PaymentDeliveryFailedQueue é obrigatório")]
        public string PaymentDeliveryFailedQueue { get; set; } = string.Empty;
    }
}

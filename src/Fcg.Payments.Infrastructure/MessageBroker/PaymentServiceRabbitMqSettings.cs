using Fcg.Core.WebApi.MessageBroker;
using System.ComponentModel.DataAnnotations;

namespace Fcg.Payments.Infrastructure.MessageBroker
{
    public class PaymentServiceRabbitMqSettings : RabbitMqConnectionSettings
    {
        [Required(ErrorMessage = "PaymentOrderQueue é obrigatório")]
        public string PaymentOrderQueue { get; set; } = string.Empty;
        [Required(ErrorMessage = "PaymentDeliveryFailedQueue é obrigatório")]
        public string PaymentDeliveryFailedQueue { get; set; } = string.Empty;
    }
}

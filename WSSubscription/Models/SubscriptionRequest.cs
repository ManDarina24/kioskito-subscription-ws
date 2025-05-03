using System.ComponentModel.DataAnnotations;

namespace WSSubscription.Models
{
    public class SubscriptionRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int PlanId { get; set; }

        [Required]
        public string PaymentMethodId { get; set; }
    }
}

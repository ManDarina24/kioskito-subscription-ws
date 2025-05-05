using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WSSubscription.Models
{
    public class CancelSubscriptionRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int PlanId { get; set; }

    }
}

namespace WSSubscription.Models
{
    public class CancelSubscriptionResponse
    { 
        public bool Success { get; set; }
        public string Message { get; set; }
        public string SubscriptionId { get; set; }
        public string Status { get; set; }

    }
}


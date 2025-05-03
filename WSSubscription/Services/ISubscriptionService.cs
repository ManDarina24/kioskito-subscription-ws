using WSSubscription.Models;

namespace WSSubscription.Services
{
    public interface ISubscriptionService
    {
        Task<SubscriptionResponse> CreateSubscriptionAsync(SubscriptionRequest request);
    }
}

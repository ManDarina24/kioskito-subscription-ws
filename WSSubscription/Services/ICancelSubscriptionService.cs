using WSSubscription.Models;


namespace WSSubscription.Services
{
    public interface ICancelSubscriptionService
    {
        Task<CancelSubscriptionResponse> CancelAsync(CancelSubscriptionRequest request);
    }
}

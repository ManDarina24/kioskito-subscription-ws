using Stripe;

namespace WSSubscription.Services
{
    public interface ISubscriptionWebhookService
    {
        Task HandleSuccessfulPayment(Invoice invoice);
        Task HandleFinalizedCancellation(Subscription subscription);
    }
}

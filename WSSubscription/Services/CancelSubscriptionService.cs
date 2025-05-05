using Stripe;
using WSSubscription.Entities;
using WSSubscription.Models;
using WSSuscripcion.Data;

namespace WSSubscription.Services
{
    public class CancelSubscriptionService : ICancelSubscriptionService
    {
        private readonly AppDbContext _context;

        public CancelSubscriptionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CancelSubscriptionResponse> CancelAsync(CancelSubscriptionRequest request)
        {
            try
            {
                //Buscar
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return new CancelSubscriptionResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                //StripeCustomerId
                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    return new CancelSubscriptionResponse
                    {
                        Success = false,
                        Message = "User has no assiciated Stripe customer ID"
                    };
                }

                //Suscripcion activa
                var subscription = _context.Suscriptions.FirstOrDefault(s => s.UserId == user.Id && s.PlanId == request.PlanId);
                if (subscription == null)
                {
                    return new CancelSubscriptionResponse
                    {
                        Success = false,
                        Message = "No subscription found for this user and plan"
                    };
                }

                //ya esta cancelada
                if(subscription.Status == "canceled" || subscription.CancelAtPeriodEnd)
                {
                    return new CancelSubscriptionResponse
                    {
                        Success = false,
                        Message = "The subscription is already canceled and does not need to be canceled again",
                        SubscriptionId = subscription.StripeSubscriptionId,
                        Status = subscription.Status
                    };
                }

                //cancelar en stripe
                var stripeService = new Stripe.SubscriptionService();
                var updatedSubscription = await stripeService.UpdateAsync(
                    subscription.StripeSubscriptionId,
                    new SubscriptionUpdateOptions
                    {
                        CancelAtPeriodEnd = true
                    });

                //actualizar db
                subscription.CancelAtPeriodEnd = true;
                subscription.CanceledAt = DateTime.UtcNow;

                _context.Suscriptions.Update(subscription);
                await _context.SaveChangesAsync();

                //repuesta good
                return new CancelSubscriptionResponse
                {
                    Success = true,
                    Message = "Subscription cancellation scheduled at period end.",
                    SubscriptionId = updatedSubscription.Id,
                    Status = updatedSubscription.Status
                };

            }
            catch (StripeException ex)
            {
                return new CancelSubscriptionResponse
                {
                    Success = false,
                    Message = $"Stripe error: {ex.Message} | Stack: {ex.StackTrace}"
                };
            }
            catch (Exception ex)
            {
                return new CancelSubscriptionResponse
                {
                    Success = false,
                    Message = $"Server error: {ex.Message} | Stack: {ex.StackTrace}"
                };
            }
        }
    }
}

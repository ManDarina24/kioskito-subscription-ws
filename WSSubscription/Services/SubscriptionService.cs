using Stripe;
using WSSubscription.Entities;
using WSSubscription.Models;
using WSSuscripcion.Data;

namespace WSSubscription.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly AppDbContext _context;

        public SubscriptionService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<SubscriptionResponse> CreateSubscriptionAsync(SubscriptionRequest request)
        {
            try
            {
                // 1. Buscar al usuario
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                    return new SubscriptionResponse { Success = false, Message = "User not found." };

                // 2. Buscar el plan
                var plan = await _context.Plans.FindAsync(request.PlanId);
                if (plan == null)
                    return new SubscriptionResponse { Success = false, Message = "Plan not found." };

                // 3. Crear cliente en Stripe si no existe
                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    var customerService = new CustomerService();
                    var customer = await customerService.CreateAsync(new CustomerCreateOptions
                    {
                        Email = user.Email,
                        Name = user.Name,
                        Source = request.PaymentMethodId
                    });

                    user.StripeCustomerId = customer.Id;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }

                // 4. Crear la suscripción en Stripe
                var stripeSubscriptionService = new Stripe.SubscriptionService();
                var subscription = await stripeSubscriptionService.CreateAsync(new SubscriptionCreateOptions
                {
                    Customer = user.StripeCustomerId,
                    Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = plan.IdPriceStripe // debe estar en tu tabla de Plans
                    }
                }
                });

                // 5. Guardar la suscripción en la base de datos
                var newSubscription = new Suscription
                {
                    UserId = user.Id,
                    PlanId = plan.Id,
                    StripeSubscriptionId = subscription.Id,
                    StripeInvoiceId = subscription.LatestInvoiceId,
                    Status = subscription.Status,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1)
                };

                _context.Suscriptions.Add(newSubscription);
                await _context.SaveChangesAsync();

                return new SubscriptionResponse
                {
                    Success = true,
                    SubscriptionId = subscription.Id,
                    Message = "Subscription created successfully."
                };
            }
            catch (StripeException ex)
            {
                return new SubscriptionResponse
                {
                    Success = false,
                    Message = $"Stripe error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new SubscriptionResponse
                {
                    Success = false,
                    Message = $"Server error: {ex.Message}"
                };
            }
        }
    
    }
}

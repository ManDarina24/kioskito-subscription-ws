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
                    if (string.IsNullOrEmpty(request.PaymentMethodId))
                    {
                        return new SubscriptionResponse
                        {
                            Success = false,
                            Message = "Payment method is required for new customers."
                        };
                    }

                    var customerService = new CustomerService();
                    var customer = await customerService.CreateAsync(new CustomerCreateOptions
                    {
                        Email = user.Email,
                        Name = user.Name,
                        PaymentMethod = request.PaymentMethodId,
                        InvoiceSettings = new CustomerInvoiceSettingsOptions
                        {
                            DefaultPaymentMethod = request.PaymentMethodId
                        }
                    });

                    user.StripeCustomerId = customer.Id;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }

                // 4. Verificar si ya tiene una suscripción activa
                var existingSubscription = _context.Suscriptions
                    .FirstOrDefault(s => s.UserId == user.Id && s.Status == "active");

                if (existingSubscription != null)
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "User already has an active subscription."
                    };
                }

                // 5. Validar que el cliente tenga un método predeterminado si no se envió uno nuevo
                string paymentMethodToUse = request.PaymentMethodId;

                if (string.IsNullOrEmpty(paymentMethodToUse))
                {
                    var customerService = new CustomerService();
                    var customer = await customerService.GetAsync(user.StripeCustomerId);

                    if (string.IsNullOrEmpty(customer.InvoiceSettings?.DefaultPaymentMethodId))
                    {
                        return new SubscriptionResponse
                        {
                            Success = false,
                            Message = "No payment method found for this customer."
                        };
                    }
                }

                // 6. Crear la suscripción en Stripe 
                var stripeSubscriptionService = new Stripe.SubscriptionService();
                var subscription = await stripeSubscriptionService.CreateAsync(new SubscriptionCreateOptions
                {
                    Customer = user.StripeCustomerId,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Price = plan.IdPriceStripe
                        }
                    },
                    Expand = new List<string> { "latest_invoice" }
                });

                // 7. Guardar la suscripción en la base de datos
                var newSubscription = new Suscription
                {
                    UserId = user.Id,
                    PlanId = plan.Id,
                    StripeSubscriptionId = subscription.Id,
                    StripeInvoiceId = subscription?.LatestInvoiceId ?? "no_invoice",
                    Status = subscription.Status ?? "incomplete",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
                    CanceledAt = null
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
                    Message = $"Stripe error: {ex.Message} | Stack: {ex.StackTrace}"
                };
            }
            catch (Exception ex)
            {
                return new SubscriptionResponse
                {
                    Success = false,
                    Message = $"Server error: {ex.Message} | Stack: {ex.StackTrace}"
                };
            }
        }
    }
}

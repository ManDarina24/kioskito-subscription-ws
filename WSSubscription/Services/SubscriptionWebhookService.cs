using Microsoft.EntityFrameworkCore;
using Stripe;
using WSSuscripcion.Data;

namespace WSSubscription.Services
{
    public class SubscriptionWebhookService : ISubscriptionWebhookService
    {
        private readonly AppDbContext _dbContext;

        public SubscriptionWebhookService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task HandleSuccessfulPayment(Invoice invoice)
        {
            Console.WriteLine($"Procesando factura: {invoice.Id}");

            // 1. Validar que la factura tenga un cliente asociado
            if (string.IsNullOrEmpty(invoice.CustomerId))
            {
                Console.WriteLine("Error: Factura no tiene un CustomerId válido");
                return;
            }

            // 2. Buscar el usuario en TU base de datos usando el Stripe CustomerId
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.StripeCustomerId == invoice.CustomerId); // Ajusta "StripeCustomerId" al nombre de tu columna

            if (user == null)
            {
                Console.WriteLine($"Error: Usuario con Stripe CustomerId {invoice.CustomerId} no encontrado");
                return;
            }

            // 3. Buscar la suscripción asociada al usuario (ajusta según tu modelo)
            var subscription = await _dbContext.Suscriptions
                .FirstOrDefaultAsync(s => s.UserId == user.Id); // Relación User -> Subscription

            if (subscription != null)
            {
                // 4. Actualizar la suscripción
                subscription.StartDate = DateTime.UtcNow;
                subscription.EndDate = DateTime.UtcNow.AddMonths(1); // Ajusta la lógica de fecha según tu negocio
                subscription.Status = "active";
                subscription.CanceledAt = null;

                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"Suscripción actualizada para el usuario: {user.Email}");
            }
            else
            {
                Console.WriteLine("Advertencia: No se encontró una suscripción asociada al usuario");
            }
        }

        public async Task HandleFinalizedCancellation(Subscription subscription)
        {
            var dbSubscription = await _dbContext.Suscriptions
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscription.Id);

            if (dbSubscription != null)
            {
                dbSubscription.Status = "canceled";
                dbSubscription.CanceledAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"Suscripción {dbSubscription.Id} cancelada");
            }
        }
    }
}
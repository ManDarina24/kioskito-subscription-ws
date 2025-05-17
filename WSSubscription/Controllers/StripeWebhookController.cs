using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WSSubscription.Services;

namespace WSSubscription.Controllers
{
    [ApiController]
    [Route("api/webhooks/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ISubscriptionWebhookService _webhookService;

        public StripeWebhookController(
            IConfiguration config,
            ISubscriptionWebhookService webhookService)
        {
            _config = config;
            _webhookService = webhookService;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            Console.WriteLine("Webhook de Stripe recibido");

            using var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();
            var secret = _config["Stripe:WebhookSecret"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    secret
                );

                Console.WriteLine($"Evento tipo: {stripeEvent.Type}");

                switch (stripeEvent.Type)
                {
                    case "invoice.payment_succeeded":

                        var invoice = stripeEvent.Data.Object as Invoice;
                        if (invoice?.CustomerId != null) // Cambio clave: Validar CustomerId en lugar de SubscriptionId
                            {
                                await _webhookService.HandleSuccessfulPayment(invoice);
                            }
                        else
                            {
                                Console.WriteLine("Advertencia: Factura sin CustomerId asociado");
                            }
                        break;

                    case "customer.subscription.deleted":
                        var subscription = stripeEvent.Data.Object as Subscription;
                        if (subscription != null)
                        {
                            await _webhookService.HandleFinalizedCancellation(subscription);
                        }
                        break;

                    default:
                        Console.WriteLine($"Evento no manejado: {stripeEvent.Type}");
                        break;
                }

                return Ok();
            }
            catch (StripeException e)
            {
                Console.WriteLine($"Error de Stripe: {e.Message}");
                return BadRequest();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error inesperado: {e}");
                return StatusCode(500);
            }
        }
    }
}
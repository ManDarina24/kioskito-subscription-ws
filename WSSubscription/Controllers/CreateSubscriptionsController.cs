using Microsoft.AspNetCore.Mvc;
using WSSubscription.Models;
using WSSubscription.Services;

namespace WSSubscription.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Esto define la ruta: api/subscriptions
    public class CreateSubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public CreateSubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new SubscriptionResponse
                {
                    Success = false,
                    Message = "Invalid request data."
                });
            }

            var result = await _subscriptionService.CreateSubscriptionAsync(request);

            if (!result.Success)
                return BadRequest(result); // Algo salió mal (usuario no encontrado, plan inválido, Stripe falló, etc.)

            return Ok(result); // Todo salió bien
        }
    }
}

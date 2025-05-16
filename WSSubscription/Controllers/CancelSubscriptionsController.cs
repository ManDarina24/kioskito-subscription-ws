using Microsoft.AspNetCore.Mvc;
using Stripe;
using WSSubscription.Models;
using WSSubscription.Services;
namespace WSSubscription.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Esto define la ruta: api/subscriptionscancel

    public class CancelSubscriptionsController : ControllerBase
    {
        private readonly ICancelSubscriptionService _cancelSubscriptionService;

        public CancelSubscriptionsController(ICancelSubscriptionService cancelSubscriptionService)
        {
            _cancelSubscriptionService = cancelSubscriptionService;
        }


        [HttpPut]
        public async Task<IActionResult> Cancel([FromBody] CancelSubscriptionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new CancelSubscriptionResponse
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            var result = await _cancelSubscriptionService.CancelAsync(request);

            if (!result.Success)
            {
                return BadRequest(result); 
            }

            return Ok(result); 
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace MockTestLab.AI.Controllers
{
    [Route("webhook")]
    [ApiController]
    public class WebhookController : Controller
    {
        const string endpointSecret = "sk_test_51NR7B6SAaY41JUKD3wULbyP6gv2sDcV5IFdU8Wro6UF4YSRJsGNRqZx7DqUC4c091mQQPguo2seXMzA9cM98MAQt00Z0jPR3aJ";

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], endpointSecret);

                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                }
                // ... handle other event types
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }
    }
}

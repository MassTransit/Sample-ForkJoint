using ForkJoint.Application.Components;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ForkJoint.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OptimisticConcurrencyTestsController : ControllerBase
    {
        private readonly IPublishEndpoint publishEndpoint;

        public OptimisticConcurrencyTestsController(IPublishEndpoint publishEndpoint)
        {
            this.publishEndpoint = publishEndpoint;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create()
        {
            var id = NewId.NextGuid();

            await publishEndpoint.Publish<Create>(new
            {
                Id = id
            });

            await Task.Delay(5000);

            await publishEndpoint.Publish<Update>(new
            {
                Id = id
            });

            return Ok();
        }
    }
}

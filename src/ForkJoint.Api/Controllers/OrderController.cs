namespace ForkJoint.Api.Controllers
{
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models;


    [ApiController]
    [Route("[controller]")]
    public class OrderController :
        ControllerBase
    {
        readonly IRequestClient<SubmitOrder> _client;
        readonly ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger, IRequestClient<SubmitOrder> client)
        {
            _logger = logger;
            _client = client;
        }

        /// <summary>
        /// Submits an order
        /// <param name="order">The order model</param>
        /// <response code="202">The order has been accepted</response>
        /// <response code="422">The order could not be processed</response>
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(OrderModel order)
        {
            Response response = await _client.GetResponse<OrderSubmissionAccepted>(new
            {
                order.OrderId,
            });

            return response switch
            {
                (_, OrderSubmissionAccepted accepted) => Accepted(new SubmitOrderResponseModel()
                {
                    OrderId = order.OrderId
                }),
                _ => BadRequest()
            };
        }
    }
}
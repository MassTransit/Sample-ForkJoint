namespace ForkJoint.Api.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Models;


    [ApiController]
    [Route("[controller]")]
    public class OrderController :
        ControllerBase
    {
        readonly IRequestClient<SubmitOrder> _client;

        public OrderController(IRequestClient<SubmitOrder> client)
        {
            _client = client;
        }

        /// <summary>
        /// Submits an order
        /// <param name="order">The order model</param>
        /// <response code="200">The order has been completed</response>
        /// <response code="202">The order has been accepted but not yet completed</response>
        /// <response code="400">The order could not be completed</response>
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(Order order)
        {
            try
            {
                Response response = await _client.GetResponse<OrderCompleted, OrderFaulted>(new
                {
                    order.OrderId,
                    order.Burgers,
                    order.Fries,
                    order.Shakes,
                    order.FryShakes
                });

                return response switch
                {
                    (_, OrderCompleted completed) => Ok(new
                    {
                        completed.OrderId,
                        completed.Created,
                        completed.Completed,
                        LinesCompleted = completed.LinesCompleted.ToDictionary(x => x.Key, x => new
                        {
                            x.Value.Created,
                            x.Value.Completed,
                            x.Value.Description,
                        })
                    }),
                    (_, OrderFaulted faulted) => BadRequest(new
                    {
                        faulted.OrderId,
                        faulted.Created,
                        faulted.Faulted,
                        LinesCompleted = faulted.LinesCompleted.ToDictionary(x => x.Key, x => new
                        {
                            x.Value.Created,
                            x.Value.Completed,
                            x.Value.Description,
                        }),
                        LinesFaulted = faulted.LinesFaulted.ToDictionary(x => x.Key, x => new
                        {
                            Faulted = x.Value.Timestamp,
                            Reason = x.Value.GetExceptionMessages(),
                        })
                    }),
                    _ => BadRequest()
                };
            }
            catch (RequestTimeoutException)
            {
                return Accepted(new
                {
                    order.OrderId,
                    Accepted = order.Burgers.Select(x => x.BurgerId).ToArray()
                });
            }
        }
    }
}
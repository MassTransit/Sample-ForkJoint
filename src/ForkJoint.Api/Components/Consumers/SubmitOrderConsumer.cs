namespace ForkJoint.Api.Components.Consumers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Microsoft.Extensions.Logging;


    public class SubmitOrderConsumer :
        IConsumer<SubmitOrder>
    {
        readonly IRequestClient<RequestBurger> _client;
        readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer(IRequestClient<RequestBurger> client, ILogger<SubmitOrderConsumer> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            var burger = context.Message.Burgers.FirstOrDefault();
            if (burger == null)
                throw new InvalidOperationException("There were no burgers to make!");

            _logger.LogInformation("Submit Order Request ID: {RequestId}", context.RequestId);

            try
            {
                Response<BurgerCompleted, BurgerNotCompleted> response = await _client.GetResponse<BurgerCompleted, BurgerNotCompleted>(new
                {
                    context.Message.OrderId,
                    Burger = burger
                });

                if (response.Is(out Response<BurgerCompleted> completed))
                {
                    await context.RespondAsync<OrderCompleted>(new
                    {
                        context.Message.OrderId,
                        completed.Message.Burger
                    });
                }
                else if (response.Is(out Response<BurgerNotCompleted> notCompleted))
                {
                    await context.RespondAsync<OrderNotCompleted>(new
                    {
                        context.Message.OrderId,
                        context.Message.Burgers,
                        notCompleted.Message.Reason
                    });
                }
            }
            catch (RequestException exception)
            {
                await context.RespondAsync<OrderNotCompleted>(new
                {
                    context.Message.OrderId,
                    context.Message.Burgers,
                    Reason = exception.Message,
                });
            }
        }
    }
}
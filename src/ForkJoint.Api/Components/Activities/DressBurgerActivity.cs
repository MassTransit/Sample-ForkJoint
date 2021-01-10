namespace ForkJoint.Api.Components.Activities
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using MassTransit.Courier;
    using Microsoft.Extensions.Logging;


    public class DressBurgerActivity :
        IExecuteActivity<DressBurgerArguments>
    {
        readonly ILogger<DressBurgerActivity> _logger;
        readonly IRequestClient<OrderOnionRings> _onionRingClient;

        public DressBurgerActivity(ILogger<DressBurgerActivity> logger, IRequestClient<OrderOnionRings> onionRingClient)
        {
            _logger = logger;
            _onionRingClient = onionRingClient;
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<DressBurgerArguments> context)
        {
            var arguments = context.Arguments;

            var patty = arguments.Patty;
            if (patty == null)
                throw new ArgumentNullException(nameof(arguments.Patty));

            _logger.LogDebug("Dressing Burger: {OrderId} {Ketchup} {Lettuce}", arguments.OrderId, arguments.Ketchup,
                arguments.Lettuce);

            if (arguments.Lettuce)
                throw new InvalidOperationException("No lettuce available");

            if (arguments.OnionRing)
            {
                _logger.LogDebug("Ordering Onion Ring: {OrderId}", arguments.BurgerId);

                Response<OnionRingsCompleted, OnionRingsFaulted> response =
                    await _onionRingClient.GetResponse<OnionRingsCompleted, OnionRingsFaulted>(new
                    {
                        arguments.OrderId,
                        OrderLineId = arguments.BurgerId,
                        Quantity = 1
                    }, context.CancellationToken);

                if (response.Is(out Response<OnionRingsFaulted> faulted))
                    throw new InvalidOperationException($"The onion ring was not available: {faulted.Message.ExceptionInfo?.Message}");
            }

            var burger = new Burger
            {
                BurgerId = arguments.BurgerId,
                Weight = patty.Weight,
                Cheese = patty.Cheese,
                Lettuce = arguments.Lettuce,
                Onion = arguments.Onion,
                Pickle = arguments.Pickle,
                Ketchup = arguments.Ketchup,
                Mustard = arguments.Mustard,
                BarbecueSauce = arguments.BarbecueSauce,
                OnionRing = arguments.OnionRing
            };

            return context.CompletedWithVariables(new {burger});
        }
    }
}
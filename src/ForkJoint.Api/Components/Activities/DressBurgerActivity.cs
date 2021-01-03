namespace ForkJoint.Api.Components.Activities
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit.Courier;
    using Microsoft.Extensions.Logging;


    public class DressBurgerActivity :
        IExecuteActivity<DressBurgerArguments>
    {
        readonly ILogger<DressBurgerActivity> _logger;

        public DressBurgerActivity(ILogger<DressBurgerActivity> logger)
        {
            _logger = logger;
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

            await Task.Delay(1000);

            var burger = new Burger
            {
                BurgerId = arguments.BurgerId,
                Weight = patty.Weight,
                Cheese = patty.Cheese,
                Lettuce = arguments.Lettuce,
                Onion = arguments.Onion,
                Pickle = arguments.Pickle,
                Ketchup = arguments.Ketchup,
                Mustard = arguments.Mustard
            };

            return context.CompletedWithVariables(new {burger});
        }
    }
}
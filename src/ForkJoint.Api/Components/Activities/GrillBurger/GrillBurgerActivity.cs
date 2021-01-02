namespace ForkJoint.Api.Components.Activities.GrillBurger
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit.Courier;
    using Microsoft.Extensions.Logging;


    public class GrillBurgerActivity :
        IActivity<GrillBurgerArguments, GrillBurgerLog>
    {
        static readonly HashSet<BurgerPatty> _inventory = new();

        readonly ILogger<GrillBurgerActivity> _logger;

        public GrillBurgerActivity(ILogger<GrillBurgerActivity> logger)
        {
            _logger = logger;
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<GrillBurgerArguments> context)
        {
            _logger.LogDebug("Grilling Burger: {OrderId} {Weight}", context.Arguments.OrderId, context.Arguments.Weight);

            var patty = await CookOrPullOutOfWarmer(context);

            return context.CompletedWithVariables<GrillBurgerLog>(new
            {
                context.Arguments.Weight,
                context.Arguments.Temperature,
                context.Arguments.Cheese
            }, new {patty});
        }

        public async Task<CompensationResult> Compensate(CompensateContext<GrillBurgerLog> context)
        {
            _logger.LogDebug("Putting Burger back in inventory: {Weight}", context.Log.Weight);

            var patty = new BurgerPatty
            {
                Weight = context.Log.Weight,
                Temperature = context.Log.Temperature,
                Cheese = context.Log.Cheese
            };

            _inventory.Add(patty);

            return context.Compensated();
        }

        async Task<BurgerPatty> CookOrPullOutOfWarmer(ExecuteContext<GrillBurgerArguments> context)
        {
            var existing = _inventory.FirstOrDefault(x => x.Cheese == context.Arguments.Cheese && x.Weight == context.Arguments.Weight);
            if (existing != null)
            {
                _logger.LogDebug("Using existing nasty patty for order", context.Arguments.OrderId);

                _inventory.Remove(existing);
                return existing;
            }

            await Task.Delay(5000);

            var patty = new BurgerPatty
            {
                Weight = context.Arguments.Weight,
                Temperature = context.Arguments.Temperature,
                Cheese = context.Arguments.Cheese
            };
            return patty;
        }
    }
}
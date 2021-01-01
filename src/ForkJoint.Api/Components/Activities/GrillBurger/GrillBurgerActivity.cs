namespace ForkJoint.Api.Components.Activities.GrillBurger
{
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit.Courier;
    using Microsoft.Extensions.Logging;


    public class GrillBurgerActivity :
        IExecuteActivity<GrillBurgerArguments>
    {
        readonly ILogger<GrillBurgerActivity> _logger;

        public GrillBurgerActivity(ILogger<GrillBurgerActivity> logger)
        {
            _logger = logger;
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<GrillBurgerArguments> context)
        {
            _logger.LogDebug("Grilling Burger: {OrderId} {Weight}", context.Arguments.OrderId, context.Arguments.Weight);

            await Task.Delay(5000);

            var patty = new BurgerPatty();

            return context.CompletedWithVariables(new {patty});
        }
    }
}
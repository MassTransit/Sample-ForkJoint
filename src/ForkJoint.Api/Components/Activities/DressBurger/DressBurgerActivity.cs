namespace ForkJoint.Api.Components.Activities.DressBurger
{
    using System.Threading.Tasks;
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
            _logger.LogDebug("Dressing Burger: {OrderId} {Ketchup}", context.Arguments.OrderId, context.Arguments.Ketchup);

            await Task.Delay(1000);

            return context.Completed();
        }
    }
}
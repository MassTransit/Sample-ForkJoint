namespace ForkJoint.Application.Components.Activities
{
    using ForkJoint.Application.Services;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Definition;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

    public class GrillBurgerActivity :
        IActivity<GrillBurgerArguments, GrillBurgerLog>
    {
        readonly IGrill _grill;
        readonly ILogger<GrillBurgerActivity> _logger;

        public GrillBurgerActivity(ILogger<GrillBurgerActivity> logger, IGrill grill)
        {
            _logger = logger;
            _grill = grill;
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<GrillBurgerArguments> context)
        {
            var patty = await _grill.CookOrUseExistingPatty(context.Arguments.Weight, context.Arguments.Cheese);

            return context.CompletedWithVariables<GrillBurgerLog>(new { patty }, new { patty });
        }

        public Task<CompensationResult> Compensate(CompensateContext<GrillBurgerLog> context)
        {
            var patty = context.Log.Patty;

            _logger.LogDebug("Putting Burger back in inventory: {Weight} {Cheese}", patty.Weight, patty.Cheese);

            _grill.Add(patty);

            return Task.FromResult(context.Compensated());
        }
    }

    public class GrillBurgerActivityefinition : ActivityDefinition<GrillBurgerActivity, GrillBurgerArguments, GrillBurgerLog>
    {
        public GrillBurgerActivityefinition()
        {
            ConcurrentMessageLimit = ConcurrentMessageLimits.GlobalValue;
        }

        protected override void ConfigureExecuteActivity(IReceiveEndpointConfigurator endpointConfigurator, IExecuteActivityConfigurator<GrillBurgerActivity, GrillBurgerArguments> executeActivityConfigurator)
        {
            endpointConfigurator.UseMessageRetry(cfg => cfg.Immediate(5));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
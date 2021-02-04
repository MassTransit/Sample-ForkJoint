namespace ForkJoint.Components.Registration
{
    using Automatonymous;
    using GreenPipes;
    using MassTransit;


    public class DefaultFutureDefinition<TFuture> :
        FutureDefinition<TFuture>
        where TFuture : MassTransitStateMachine<FutureState>
    {
        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<FutureState> sagaConfigurator)
        {
            endpointConfigurator.UseScheduledRedelivery(r => r.Intervals(5000, 30000, 120000));
            endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
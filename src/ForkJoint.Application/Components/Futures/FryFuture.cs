namespace ForkJoint.Application.Components.Futures
{
    using Contracts;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Futures;
    using MassTransit.Registration;

    public class FryFuture :
        Future<OrderFry, FryCompleted>
    {
        public FryFuture()
        {
            ConfigureCommand(x => x.CorrelateById(context => context.Message.OrderLineId));

            SendRequest<CookFry>(x =>
                {
                    x.UsingRequestFactory(context => new CookFryRequest(context.Message.OrderId, context.Message.OrderLineId, context.Message.Size));
                })
                .OnResponseReceived<FryReady>(x =>
                {
                    x.SetCompletedUsingFactory(context => new FryCompletedResult(context.Instance.Created,
                        context.Instance.Completed ?? default,
                        context.Message.OrderId,
                        context.Message.OrderLineId,
                        context.Message.Size,
                        $"{context.Message.Size} Fries"));
                });
        }
    }

    public class FryFutureDefinition : FutureDefinition<FryFuture>
    {
        public FryFutureDefinition()
        {
            ConcurrentMessageLimit = ConcurrentMessageLimits.GlobalValue;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<FutureState> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(cfg => cfg.Immediate(5));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
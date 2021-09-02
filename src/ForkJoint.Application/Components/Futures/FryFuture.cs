namespace ForkJoint.Application.Components.Futures
{
    using Contracts;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Futures;
using MassTransit.RabbitMqTransport;
    using MassTransit.Registration;
    using System;

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
            ConcurrentMessageLimit = GlobalValues.ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<FutureState> sagaConfigurator)
        {
            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator)
            {
                var rabbitMqReceiveEndpointConfigurator = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

                if (GlobalValues.PrefetchCount != null)
                    rabbitMqReceiveEndpointConfigurator.PrefetchCount = (int)GlobalValues.PrefetchCount;

                if (GlobalValues.UseQuorumQueues)
                    rabbitMqReceiveEndpointConfigurator.SetQuorumQueue();

                if (GlobalValues.UseLazyQueues && !GlobalValues.UseQuorumQueues)
                    rabbitMqReceiveEndpointConfigurator.Lazy = GlobalValues.UseLazyQueues;
            }

            endpointConfigurator.UseMessageRetry(cfg => cfg.Intervals(500, 15000, 60000, 120000));

            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}

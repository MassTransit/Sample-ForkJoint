namespace ForkJoint.Application.Components.Futures
{
    using Contracts;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Futures;
using MassTransit.RabbitMqTransport;
    using MassTransit.Registration;
    using System;

    public class FryShakeFuture :
        Future<OrderFryShake, FryShakeCompleted>
    {
        public FryShakeFuture()
        {
            ConfigureCommand(x => x.CorrelateById(context => context.Message.OrderLineId));

            SendRequest<OrderFry>(x =>
                {
                    x.UsingRequestInitializer(context => new
                    {
                        OrderId = context.Instance.CorrelationId,
                        OrderLineId = InVar.Id,
                        context.Message.Size,
                    });

                    x.TrackPendingRequest(message => message.OrderLineId);
                })
                .OnResponseReceived<FryCompleted>(x =>
                {
                    x.CompletePendingRequest(message => message.OrderLineId);
                });

            SendRequest<OrderShake>(x =>
                {
                    x.UsingRequestInitializer(context => new
                    {
                        OrderId = context.Instance.CorrelationId,
                        OrderLineId = InVar.Id,
                        context.Message.Flavor,
                        context.Message.Size,
                    });

                    x.TrackPendingRequest(message => message.OrderLineId);
                })
                .OnResponseReceived<ShakeCompleted>(x =>
                {
                    x.CompletePendingRequest(message => message.OrderLineId);
                });


            WhenAllCompleted(x => x.SetCompletedUsingInitializer(context =>
            {
                var message = context.Instance.GetCommand<OrderFryShake>();

                return new { Description = $"{message.Size} {message.Flavor} FryShake({context.Instance.Results.Count})" };
            }));
        }
    }

    public class FryShakeFutureDefinition : FutureDefinition<FryShakeFuture>
    {
        public FryShakeFutureDefinition()
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
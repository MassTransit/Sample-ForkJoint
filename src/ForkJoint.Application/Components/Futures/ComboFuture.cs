namespace ForkJoint.Application.Components.Futures
{
    using Contracts;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Futures;
using MassTransit.RabbitMqTransport;
    using MassTransit.Registration;
    using System;
    using System.Linq;

    public class ComboFuture :
        Future<OrderCombo, ComboCompleted>
    {
        public ComboFuture()
        {
            Event(() => CommandReceived, x => x.CorrelateById(context => context.Message.OrderLineId));

            var fry = SendRequest<OrderFry>(x =>
                {
                    x.UsingRequestInitializer(context => new
                    {
                        OrderId = context.Instance.CorrelationId,
                        OrderLineId = InVar.Id,
                        Size = Size.Medium
                    });

                    x.TrackPendingRequest(message => message.OrderLineId);
                })
                .OnResponseReceived<FryCompleted>(x => x.CompletePendingRequest(message => message.OrderLineId));

            var shake = SendRequest<OrderShake>(x =>
                {
                    x.UsingRequestInitializer(context => new
                    {
                        OrderId = context.Instance.CorrelationId,
                        OrderLineId = InVar.Id,
                        Size = Size.Medium,
                        Flavor = "Chocolate",
                    });

                    x.TrackPendingRequest(message => message.OrderLineId);
                })
                .OnResponseReceived<ShakeCompleted>(x => x.CompletePendingRequest(message => message.OrderLineId));


            WhenAllCompleted(x =>
            {
                x.SetCompletedUsingInitializer(context =>
                {
                    var fryCompleted = context.SelectResults<FryCompleted>().FirstOrDefault();
                    var shakeCompleted = context.SelectResults<ShakeCompleted>().FirstOrDefault();

                    return new { Description = $"Combo ({fryCompleted.Description}, {shakeCompleted.Description})" };
                });
            });
        }
    }

    public class ComboFutureDefinition : FutureDefinition<ComboFuture>
    {
        public ComboFutureDefinition()
        {
            ConcurrentMessageLimit = GlobalValues.ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<FutureState> sagaConfigurator)
        {
            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator && GlobalValues.UseLazyQueues)
            {
                ((IRabbitMqReceiveEndpointConfigurator)endpointConfigurator).Lazy = GlobalValues.UseLazyQueues;
            }

            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator && GlobalValues.PrefetchCount != null)
            {
                ((IRabbitMqReceiveEndpointConfigurator)endpointConfigurator).PrefetchCount = (int)GlobalValues.PrefetchCount;
            }

            endpointConfigurator.UseMessageRetry(cfg => cfg.Intervals(500, 15000, 60000));

            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
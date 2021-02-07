namespace ForkJoint.Api.Components.Futures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using MassTransit;
    using MassTransit.Futures;


    public class OrderFuture :
        Future<SubmitOrder, OrderCompleted, OrderFaulted>
    {
        public OrderFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderId));

            SendRequests<Burger, OrderBurger, BurgerCompleted>(x => x.Burgers, x =>
            {
                x.Pending(m => m.OrderLineId, m => m.OrderLineId);
                x.Command(c =>
                {
                    c.Init(context => new
                    {
                        OrderId = context.Instance.CorrelationId,
                        OrderLineId = context.Message.BurgerId,
                        Burger = context.Message
                    });
                });
            });

            SendRequests<Fry, OrderFry, FryCompleted>(x => x.Fries, x =>
            {
                x.Pending(m => m.OrderLineId, m => m.OrderLineId);
                x.Command(c =>
                {
                    c.Init(context => new
                    {
                        OrderId = context.Instance.CorrelationId,
                        OrderLineId = context.Message.FryId,
                        context.Message.Size,
                    });
                });
            });

            SendRequests<Shake, OrderShake, ShakeCompleted>(x => x.Shakes, x =>
            {
                x.Pending(m => m.OrderLineId, m => m.OrderLineId);
                x.Command(c =>
                {
                    c.Init(context => new
                    {
                        OrderId = context.Instance.CorrelationId,
                        OrderLineId = context.Message.ShakeId,
                        context.Message.Size,
                        context.Message.Flavor
                    });
                });
            });

            SendRequests<FryShake, OrderFryShake, FryShakeCompleted>(x => x.FryShakes, x =>
            {
                x.Pending(m => m.OrderLineId, m => m.OrderLineId);
                x.Command(c =>
                {
                    c.Init(context => new
                    {
                        OrderId = context.Instance.CorrelationId,
                        OrderLineId = context.Message.FryShakeId,
                        context.Message.Size,
                        context.Message.Flavor
                    });
                });
            });

            Response(r => r.Init(context => new
            {
                LinesCompleted = context.Instance.Results.Select(x => x.Value.ToObject<OrderLineCompleted>()).ToDictionary(x => x.OrderLineId),
            }));

            Fault(f => f.Init(context =>
            {
                Dictionary<Guid, Fault> faults = context.Instance.Faults.ToDictionary(x => x.Key, x => x.Value.ToObject<Fault>());

                return new
                {
                    LinesCompleted = context.Instance.Results.ToDictionary(x => x.Key, x => x.Value.ToObject<OrderLineCompleted>()),
                    LinesFaulted = faults,
                    Exceptions = faults.SelectMany(x => x.Value.Exceptions).ToArray()
                };
            }));
        }
    }
}

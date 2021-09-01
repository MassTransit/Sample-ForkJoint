namespace ForkJoint.Application.Components.ItineraryPlanners
{
    using Contracts;
    using ForkJoint.Application.Components.Activities;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Futures;
    using System;
    using System.Threading.Tasks;

    public class BurgerItineraryPlanner :
        IItineraryPlanner<OrderBurger>
    {
        readonly Uri _dressAddress;
        readonly Uri _grillAddress;

        public BurgerItineraryPlanner(IEndpointNameFormatter formatter)
        {
            _grillAddress = new Uri($"exchange:{formatter.ExecuteActivity<GrillBurgerActivity, GrillBurgerArguments>()}");
            _dressAddress = new Uri($"exchange:{formatter.ExecuteActivity<DressBurgerActivity, DressBurgerArguments>()}");
        }

        public async Task PlanItinerary(FutureConsumeContext<OrderBurger> context, ItineraryBuilder builder)
        {
            var orderBurger = context.Message;

            builder.AddVariable(nameof(OrderBurger.OrderId), orderBurger.OrderId);
            builder.AddVariable(nameof(OrderBurger.OrderLineId), orderBurger.OrderLineId);

            var burger = orderBurger.Burger;

            builder.AddActivity(nameof(GrillBurgerActivity), _grillAddress, new
            {
                burger.Weight,
                burger.Cheese,
            });

            Guid? onionRingId = default;

            if (burger.OnionRing && GlobalValues.PreOrderOnionRings)
            {
                onionRingId = NewId.NextGuid();

                // TODO create a future with address/id
                await context.Publish<OrderOnionRings>(new
                {
                    orderBurger.OrderId,
                    OrderLineId = onionRingId,
                    Quantity = 1
                });
            }

            builder.AddActivity(nameof(DressBurgerActivity), _dressAddress, new
            {
                burger.Lettuce,
                burger.Pickle,
                burger.Onion,
                burger.Ketchup,
                burger.Mustard,
                burger.BarbecueSauce,
                burger.OnionRing,
                onionRingId
            });
        }
    }
}
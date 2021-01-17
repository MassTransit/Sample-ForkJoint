namespace ForkJoint.Api.Components.ItineraryPlanners
{
    using System;
    using System.Threading.Tasks;
    using Activities;
    using Contracts;
    using ForkJoint.Components;
    using MassTransit;
    using MassTransit.Courier;


    public class BurgerItineraryPlanner :
        IItineraryPlanner<OrderBurger>
    {
        readonly ConsumeContext _consumeContext;
        readonly Uri _dressAddress;
        readonly Uri _grillAddress;

        public BurgerItineraryPlanner(IEndpointNameFormatter formatter, ConsumeContext consumeContext)
        {
            _consumeContext = consumeContext;
            _grillAddress = new Uri($"exchange:{formatter.ExecuteActivity<GrillBurgerActivity, GrillBurgerArguments>()}");
            _dressAddress = new Uri($"exchange:{formatter.ExecuteActivity<DressBurgerActivity, DressBurgerArguments>()}");
        }

        public async Task PlanItinerary(OrderBurger orderBurger, ItineraryBuilder builder)
        {
            builder.AddVariable(nameof(OrderBurger.OrderId), orderBurger.OrderId);
            builder.AddVariable(nameof(OrderBurger.OrderLineId), orderBurger.OrderLineId);

            var burger = orderBurger.Burger;

            builder.AddActivity(nameof(GrillBurgerActivity), _grillAddress, new
            {
                burger.Weight,
                burger.Cheese,
            });

            Guid? onionRingId = default;
            if (burger.OnionRing)
            {
                onionRingId = NewId.NextGuid();

                // TODO create a future with address/id
                await _consumeContext.Publish<OrderOnionRings>(new
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
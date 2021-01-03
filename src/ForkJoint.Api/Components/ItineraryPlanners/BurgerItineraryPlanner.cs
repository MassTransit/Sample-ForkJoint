namespace ForkJoint.Api.Components.ItineraryPlanners
{
    using System;
    using Activities;
    using Contracts;
    using MassTransit;
    using MassTransit.Courier;


    public class BurgerItineraryPlanner :
        IItineraryPlanner<Burger>
    {
        readonly Uri _dressAddress;
        readonly Uri _grillAddress;

        public BurgerItineraryPlanner(IEndpointNameFormatter formatter)
        {
            _grillAddress = new Uri($"exchange:{formatter.ExecuteActivity<GrillBurgerActivity, GrillBurgerArguments>()}");
            _dressAddress = new Uri($"exchange:{formatter.ExecuteActivity<DressBurgerActivity, DressBurgerArguments>()}");
        }

        public void PlanItinerary(Burger burger, ItineraryBuilder builder)
        {
            builder.AddActivity(nameof(GrillBurgerActivity), _grillAddress, new
            {
                burger.Weight,
                burger.Cheese,
            });

            builder.AddActivity(nameof(DressBurgerActivity), _dressAddress, new
            {
                burger.Lettuce,
                burger.Pickle,
                burger.Onion,
                burger.Ketchup,
                burger.Mustard
            });
        }
    }
}
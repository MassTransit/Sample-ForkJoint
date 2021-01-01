namespace ForkJoint.Api.Components.Activities.GrillBurger
{
    using System;


    public interface GrillBurgerArguments
    {
        Guid OrderId { get; }

        decimal Weight { get; }
        decimal Temperature { get; }
        bool Cheese { get; }
    }
}
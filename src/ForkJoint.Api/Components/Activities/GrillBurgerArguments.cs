namespace ForkJoint.Api.Components.Activities
{
    using System;
    using Contracts;


    public interface GrillBurgerArguments
    {
        Guid OrderId { get; }

        BurgerPatty Patty { get; }

        decimal Weight { get; }
        bool Cheese { get; }
    }
}
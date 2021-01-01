namespace ForkJoint.Api.Components.Activities.DressBurger
{
    using System;
    using Contracts;


    public interface DressBurgerArguments
    {
        Guid OrderId { get; }

        BurgerPatty Patty { get; }

        bool Lettuce { get; }
        bool Pickles { get; }
        bool Ketchup { get; }
    }
}
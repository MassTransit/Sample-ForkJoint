namespace ForkJoint.Api.Components.Activities;

using System;


public interface GrillBurgerArguments
{
    Guid OrderId { get; }
    Guid BurgerId { get; }

    decimal Weight { get; }
    bool Cheese { get; }
}
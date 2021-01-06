namespace ForkJoint.Contracts
{
    using System;


    public interface OrderBurger
    {
        Guid OrderId { get; }

        Burger Burger { get; }
    }
}
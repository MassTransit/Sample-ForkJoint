namespace ForkJoint.Contracts
{
    using System;


    public interface OrderShake
    {
        Guid OrderId { get; }
        Guid OrderLineId { get; }

        string Flavor { get; }
        Size Size { get; }
    }
}
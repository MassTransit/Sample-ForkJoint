namespace ForkJoint.Contracts
{
    using System;


    public interface OrderOnionRings
    {
        Guid OrderId { get; }
        Guid OrderLineId { get; }

        int Quantity { get; }
    }
}
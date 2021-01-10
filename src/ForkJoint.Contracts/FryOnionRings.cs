namespace ForkJoint.Contracts
{
    using System;


    public interface FryOnionRings
    {
        Guid OrderId { get; }
        Guid OrderLineId { get; }

        int Quantity { get; }
    }
}
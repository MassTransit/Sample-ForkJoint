namespace ForkJoint.Contracts
{
    using System;


    public interface OnionRingsFried
    {
        Guid OrderId { get; }
        Guid OrderLineId { get; }
        int Quantity { get; }
    }
}
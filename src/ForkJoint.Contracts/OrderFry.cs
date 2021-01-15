namespace ForkJoint.Contracts
{
    using System;


    public interface OrderFry
    {
        Guid OrderId { get; }
        Guid OrderLineId { get; }

        Size Size { get; }
    }
}
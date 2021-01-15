namespace ForkJoint.Contracts
{
    using System;


    public interface CookFry
    {
        Guid OrderId { get; }
        Guid OrderLineId { get; }

        Size Size { get; }
    }
}
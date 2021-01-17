namespace ForkJoint.Contracts
{
    using System;
    using MassTransit.Topology;


    [ExcludeFromTopology]
    public interface OrderLine
    {
        Guid OrderId { get; }
        Guid OrderLineId { get; }
    }
}
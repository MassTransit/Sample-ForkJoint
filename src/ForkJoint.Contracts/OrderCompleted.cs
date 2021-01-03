namespace ForkJoint.Contracts
{
    using System;
    using System.Runtime.CompilerServices;
    using MassTransit;
    using MassTransit.Topology.Topologies;


    public interface OrderCompleted
    {
        Guid OrderId { get; }

        Burger Burger { get; }

        [ModuleInitializer]
        internal static void Init()
        {
            GlobalTopology.Send.UseCorrelationId<OrderCompleted>(x => x.OrderId);
        }
    }
}
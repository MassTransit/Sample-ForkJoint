namespace ForkJoint.Contracts
{
    using System;
    using System.Runtime.CompilerServices;
    using MassTransit;
    using MassTransit.Topology.Topologies;


    public interface OrderNotCompleted
    {
        Guid OrderId { get; }

        string Reason { get; }

        Burger[] Burgers { get; }

        [ModuleInitializer]
        internal static void Init()
        {
            GlobalTopology.Send.UseCorrelationId<OrderNotCompleted>(x => x.OrderId);
        }
    }
}
namespace ForkJoint.Contracts
{
    using System;
    using System.Runtime.CompilerServices;
    using MassTransit;
    using MassTransit.Topology.Topologies;


    public interface SubmitOrder
    {
        Guid OrderId { get; }

        bool Lettuce { get; }

        [ModuleInitializer]
        internal static void Init()
        {
            GlobalTopology.Send.UseCorrelationId<SubmitOrder>(x => x.OrderId);
        }
    }
}
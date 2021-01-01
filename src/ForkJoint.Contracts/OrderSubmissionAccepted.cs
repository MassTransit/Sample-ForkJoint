namespace ForkJoint.Contracts
{
    using System;
    using System.Runtime.CompilerServices;
    using MassTransit;
    using MassTransit.Topology.Topologies;


    public interface OrderSubmissionAccepted
    {
        Guid OrderId { get; }

        [ModuleInitializer]
        internal static void Init()
        {
            GlobalTopology.Send.UseCorrelationId<OrderSubmissionAccepted>(x => x.OrderId);
        }
    }
}
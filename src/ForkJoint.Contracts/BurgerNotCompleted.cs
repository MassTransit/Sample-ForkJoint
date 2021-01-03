namespace ForkJoint.Contracts
{
    using System;
    using System.Runtime.CompilerServices;
    using MassTransit;
    using MassTransit.Topology.Topologies;


    public interface BurgerNotCompleted
    {
        Guid OrderId { get; }

        string Reason { get; }

        Burger Burger { get; }

        [ModuleInitializer]
        internal static void Init()
        {
            GlobalTopology.Send.UseCorrelationId<BurgerNotCompleted>(x => x.Burger.BurgerId);
        }
    }
}
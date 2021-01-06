namespace ForkJoint.Contracts
{
    using System;
    using System.Collections.Generic;


    public interface OrderFaulted :
        FutureFaulted
    {
        Guid OrderId { get; }

        IDictionary<Guid, OrderLineCompleted> LinesCompleted { get; }

        IDictionary<Guid, OrderLineFaulted> LinesFaulted { get; }
    }
}
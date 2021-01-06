namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using System.Collections.Generic;
    using Contracts;


    public class OrderState :
        FutureState
    {
        public DateTime? Updated { get; set; }

        public int LineCount { get; set; }

        public HashSet<Guid> LinesPending { get; set; } = new();
        public Dictionary<Guid, OrderLineCompleted> LinesCompleted { get; set; } = new();
        public Dictionary<Guid, OrderLineFaulted> LinesFaulted { get; set; } = new();
    }
}
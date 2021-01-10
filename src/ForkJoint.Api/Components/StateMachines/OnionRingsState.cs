namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using MassTransit;


    public class OnionRingsState :
        FutureState
    {
        public Guid OrderId { get; set; }

        public int Quantity { get; set; }

        public ExceptionInfo ExceptionInfo { get; set; }
    }
}
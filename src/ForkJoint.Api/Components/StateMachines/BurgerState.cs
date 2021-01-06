namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using Contracts;
    using MassTransit;


    public class BurgerState :
        FutureState
    {
        public Guid OrderId { get; set; }

        public Burger Burger { get; set; }

        public Guid TrackingNumber { get; set; }

        public ExceptionInfo ExceptionInfo { get; set; }
    }
}
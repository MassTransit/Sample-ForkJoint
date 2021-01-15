namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using Contracts;
    using MassTransit;


    public class FryState :
        FutureState
    {
        public Guid OrderId { get; set; }

        public Size Size { get; set; }

        public ExceptionInfo ExceptionInfo { get; set; }
    }
}
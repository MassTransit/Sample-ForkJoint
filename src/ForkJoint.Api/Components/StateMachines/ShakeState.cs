namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using Contracts;
    using MassTransit;


    public class ShakeState :
        FutureState
    {
        public Guid OrderId { get; set; }

        public string Flavor { get; set; }
        public Size Size { get; set; }

        public ExceptionInfo ExceptionInfo { get; set; }
    }
}
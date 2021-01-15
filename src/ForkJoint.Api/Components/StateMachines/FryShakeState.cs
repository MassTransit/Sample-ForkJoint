namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using Contracts;
    using MassTransit;


    public class FryShakeState :
        FutureState
    {
        public Guid OrderId { get; set; }

        public string Flavor { get; set; }
        public Size Size { get; set; }

        public int BothState { get; set; }

        public ExceptionInfo ExceptionInfo { get; set; }
    }
}
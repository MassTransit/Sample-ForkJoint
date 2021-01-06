namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using Automatonymous;


    public class FutureState :
        SagaStateMachineInstance
    {
        public int CurrentState { get; set; }

        public Uri ResponseAddress { get; set; }

        public Guid? RequestId { get; set; }

        /// <summary>
        /// When the Future was Created
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// When the Future was Completed
        /// </summary>
        public DateTime? Completed { get; set; }

        /// <summary>
        /// When the Future was last Faulted
        /// </summary>
        public DateTime? Faulted { get; set; }

        public Guid CorrelationId { get; set; }
    }
}
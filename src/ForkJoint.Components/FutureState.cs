namespace ForkJoint.Components
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Automatonymous;
    using MassTransit.Saga;


    public class FutureState :
        SagaStateMachineInstance,
        ISagaVersion
    {
        public int CurrentState { get; set; }

        public DateTime Created { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? Completed { get; set; }
        public DateTime? Canceled { get; set; }
        public DateTime? Faulted { get; set; }

        /// <summary>
        /// A URI containing the future's address and identity.
        /// For example, queue:burger-state?id=B2D7B506-B453-48DE-A85F-BD958FB84C4F
        /// </summary>
        public Uri Location { get; set; }

        public FutureMessage Request { get; set; }
        public HashSet<Guid> Pending { get; set; } = new();
        public Dictionary<Guid, FutureMessage> Results { get; set; } = new();
        public Dictionary<Guid, FutureMessage> Faults { get; set; } = new();

        public HashSet<FutureSubscription> Subscriptions { get; set; }

        public int Version { get; set; }
        public Guid CorrelationId { get; set; }

        public void AddSubscription(Uri address, Guid? requestId = default)
        {
            if (address == null)
                return;

            Subscriptions ??= new HashSet<FutureSubscription>(FutureSubscription.Comparer);
            Subscriptions.Add(new FutureSubscription(address, requestId));
        }

        public T GetRequest<T>()
            where T : class
        {
            return Request?.ToObject<T>();
        }

        public bool TryGetResult<T>(Guid id, out T result)
            where T : class
        {
            if (Results != null && Results.TryGetValue(id, out var message))
            {
                result = message.ToObject<T>();
                return result != default;
            }

            result = default;
            return false;
        }

        public bool TryGetFault<T>(Guid id, out T fault)
            where T : class
        {
            if (Faults != null && Faults.TryGetValue(id, out var message))
            {
                fault = message.ToObject<T>();
                return fault != default;
            }

            fault = default;
            return false;
        }

        public async Task<TResult> SetResult<T, TResult>(ConsumeEventContext<FutureState, T> context, Guid id,
            AsyncEventMessageFactory<FutureState, T, TResult> factory)
            where T : class
            where TResult : class
        {
            var timestamp = context.SentTime ?? DateTime.UtcNow;

            if (Pending != null)
            {
                Pending.Remove(id);

                if (Pending.Count == 0 && Faults.Count == 0)
                    Completed = timestamp;
            }
            else if (Faults.Count == 0)
                Completed = timestamp;

            var result = await factory(context).ConfigureAwait(false);

            Results[id] = new FutureMessage<TResult>(result);

            return result;
        }

        public async Task<TFault> SetFault<T, TFault>(ConsumeEventContext<FutureState, T> context, Guid id,
            AsyncEventMessageFactory<FutureState, T, TFault> factory)
            where T : class
            where TFault : class
        {
            var timestamp = context.SentTime ?? DateTime.UtcNow;

            Pending?.Remove(id);

            Faulted ??= timestamp;

            var fault = await factory(context).ConfigureAwait(false);

            Faults[id] = new FutureMessage<TFault>(fault);

            return fault;
        }
    }
}
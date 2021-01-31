namespace ForkJoint.Components
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using MassTransit;


    public static class FutureStateExtensions
    {
        public static T GetRequest<T>(this FutureState future)
            where T : class
        {
            return future.Request?.ToObject<T>();
        }

        public static void AddSubscription(this FutureState future, ConsumeContext context)
        {
            if (context.ResponseAddress == null)
                return;

            future.Subscriptions.Add(new FutureSubscription(context.ResponseAddress, context.RequestId));
        }

        public static async Task<TResult> SetResult<T, TResult>(this FutureState future,
            ConsumeEventContext<FutureState, T> context, Guid id, AsyncEventMessageFactory<FutureState, T, TResult> factory)
            where T : class
            where TResult : class
        {
            var timestamp = context.SentTime ?? DateTime.UtcNow;

            if (future.HasPending())
            {
                future.Pending.Remove(id);

                if (!future.HasPending() && !future.HasFaults())
                    future.Completed = timestamp;
            }
            else if (!future.HasFaults())
                future.Completed = timestamp;

            var result = await factory(context).ConfigureAwait(false);

            future.Results[id] = new FutureMessage<TResult>(result);

            return result;
        }

        public static void SetResult<T, TResult>(this FutureConsumeContext<T> context, Guid id, TResult result)
            where T : class
            where TResult : class
        {
            if (!context.Instance.Completed.HasValue)
                SetCompleted(context, id);

            context.Instance.Results[id] = new FutureMessage<TResult>(result);
        }

        public static void SetCompleted<T>(this FutureConsumeContext<T> context, Guid id)
            where T : class
        {
            var timestamp = context.SentTime ?? DateTime.UtcNow;

            var future = context.Instance;

            if (future.HasPending())
            {
                future.Pending.Remove(id);

                if (!future.HasPending() && !future.HasFaults())
                    future.Completed = timestamp;
            }
            else if (!future.HasFaults())
                future.Completed = timestamp;
        }

        public static void SetFaulted(this FutureConsumeContext context, Guid id, DateTime? timestamp = default)
        {
            timestamp ??= context.SentTime ?? DateTime.UtcNow;

            var future = context.Instance;

            if (future.HasPending())
                future.Pending?.Remove(id);

            future.Faulted ??= timestamp;
        }

        public static void SetFault<T, TFault>(this FutureConsumeContext<T> context, Guid id, TFault fault, DateTime? timestamp = default)
            where T : class
            where TFault : class
        {
            if (!context.Instance.Faulted.HasValue)
                SetFaulted(context, id, timestamp);

            context.Instance.Faults[id] = new FutureMessage<TFault>(fault);
        }

        public static async Task<TFault> SetFault<T, TFault>(this FutureState future,
            ConsumeEventContext<FutureState, T> context, Guid id, AsyncEventMessageFactory<FutureState, T, TFault> factory)
            where T : class
            where TFault : class
        {
            var timestamp = context.SentTime ?? DateTime.UtcNow;

            if (future.HasPending())
                future.Pending?.Remove(id);

            future.Faulted ??= timestamp;

            var fault = await factory(context).ConfigureAwait(false);

            future.Faults[id] = new FutureMessage<TFault>(fault);

            return fault;
        }

        public static bool TryGetResult<T>(this FutureState future, Guid id, out T result)
            where T : class
        {
            if (future.HasResults() && future.Results.TryGetValue(id, out var message))
            {
                result = message.ToObject<T>();
                return result != default;
            }

            result = default;
            return false;
        }

        public static bool TryGetFault<T>(this FutureState future, Guid id, out T fault)
            where T : class
        {
            if (future.HasFaults() && future.Faults.TryGetValue(id, out var message))
            {
                fault = message.ToObject<T>();
                return fault != default;
            }

            fault = default;
            return false;
        }
    }
}
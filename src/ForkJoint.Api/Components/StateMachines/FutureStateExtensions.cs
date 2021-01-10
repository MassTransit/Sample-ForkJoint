namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using Automatonymous;
    using Automatonymous.Binders;


    public static class FutureStateExtensions
    {
        /// <summary>
        /// Initialize the FutureState properties of the request
        /// </summary>
        /// <param name="binder"></param>
        /// <typeparam name="TState"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static EventActivityBinder<TState, T> InitializeFuture<TState, T>(this EventActivityBinder<TState, T> binder)
            where TState : FutureState
            where T : class
        {
            return binder.Then(context =>
            {
                context.Instance.Created = DateTime.UtcNow;

                ConsumeEventContext<TState, T> consumeContext = context.CreateConsumeContext();

                context.Instance.RequestId = consumeContext.RequestId;
                context.Instance.ResponseAddress = consumeContext.ResponseAddress;
            });
        }

        /// <summary>
        /// Complete the request and respond to the request originator along with any other pending requests for the same future result.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="asyncMessageFactory">The response message factory</param>
        /// <typeparam name="TState">The FutureState type</typeparam>
        /// <typeparam name="T">The event message type</typeparam>
        /// <typeparam name="TResponse">The response message type</typeparam>
        /// <returns></returns>
        public static EventActivityBinder<TState, T> SetCompleted<TState, T, TResponse>(this EventActivityBinder<TState, T> binder,
            AsyncEventMessageFactory<TState, T, TResponse> asyncMessageFactory)
            where TState : FutureState
            where T : class
            where TResponse : class
        {
            return binder
                .Then(context => context.Instance.Completed = context.CreateConsumeContext().SentTime ?? DateTime.UtcNow)
                .If(context => context.Instance.RequestId.HasValue && context.Instance.ResponseAddress != null,
                    respond => respond.SendAsync(context => context.Instance.ResponseAddress, asyncMessageFactory, (consumeContext, sendContext) =>
                    {
                        sendContext.RequestId = consumeContext.Instance.RequestId;
                    }))
                .RequestCompleted(asyncMessageFactory);
        }

        /// <summary>
        /// Complete the request and respond to the request originator along with any other pending requests for the same future result.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="asyncMessageFactory">The response message factory</param>
        /// <typeparam name="TState">The FutureState type</typeparam>
        /// <typeparam name="T">The event message type</typeparam>
        /// <typeparam name="TResponse">The response message type</typeparam>
        /// <returns></returns>
        public static EventActivityBinder<TState, T> SetFaulted<TState, T, TResponse>(this EventActivityBinder<TState, T> binder,
            AsyncEventMessageFactory<TState, T, TResponse> asyncMessageFactory)
            where TState : FutureState
            where T : class
            where TResponse : class
        {
            return binder
                .Then(context => context.Instance.Faulted = context.CreateConsumeContext().SentTime ?? DateTime.UtcNow)
                .If(context => context.Instance.RequestId.HasValue && context.Instance.ResponseAddress != null,
                    respond => respond.SendAsync(context => context.Instance.ResponseAddress, asyncMessageFactory, (consumeContext, sendContext) =>
                    {
                        sendContext.RequestId = consumeContext.Instance.RequestId;
                    }))
                .RequestCompleted(asyncMessageFactory);
        }

        /// <summary>
        /// Use when a request is received after the initial request is still awaiting completion
        /// </summary>
        /// <param name="binder"></param>
        /// <typeparam name="TState"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static EventActivityBinder<TState, T> PendingRequestStarted<TState, T>(this EventActivityBinder<TState, T> binder)
            where TState : FutureState
            where T : class
        {
            return binder.If(x => x.Instance.RequestId != x.CreateConsumeContext().RequestId, x => x.RequestStarted());
        }
    }
}
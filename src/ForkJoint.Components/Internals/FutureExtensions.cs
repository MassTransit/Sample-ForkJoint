namespace ForkJoint.Components.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using Automatonymous.Binders;
    using Automatonymous.Contexts;
    using GreenPipes;
    using MassTransit;


    public static class FutureExtensions
    {
        /// <summary>
        /// Initialize the FutureState properties of the request
        /// </summary>
        /// <param name="binder"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static EventActivityBinder<FutureState, T> InitializeFuture<T>(this EventActivityBinder<FutureState, T> binder)
            where T : class
        {
            return binder
                .Then(context =>
                {
                    context.Instance.Created = DateTime.UtcNow;
                    context.Instance.Request = new FutureMessage<T>(context.Data);
                    context.Instance.Location = new FutureLocation(context.Instance.CorrelationId, context.CreateConsumeContext().ReceiveContext.InputAddress);
                })
                .AddSubscription();
        }

        /// <summary>
        /// Use when a request is received after the initial request is still awaiting completion
        /// </summary>
        /// <param name="binder"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static EventActivityBinder<FutureState, T> AddSubscription<T>(this EventActivityBinder<FutureState, T> binder)
            where T : class
        {
            return binder.Then(context =>
            {
                ConsumeEventContext<FutureState, T> consumeContext = context.CreateConsumeContext();
                context.Instance.AddSubscription(consumeContext);
            });
        }

        /// <summary>
        /// Set the result associated with the identifier using the message factory
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="getResultId">Should return the result identifier</param>
        /// <param name="messageFactory">Should return the result message</param>
        /// <typeparam name="T">The event type</typeparam>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <returns></returns>
        public static EventActivityBinder<FutureState, T> SetResult<T, TResult>(this EventActivityBinder<FutureState, T> binder,
            Func<ConsumeEventContext<FutureState, T>, Guid> getResultId, EventMessageFactory<FutureState, T, TResult> messageFactory)
            where T : class
            where TResult : class
        {
            return binder.ThenAsync(context =>
            {
                ConsumeEventContext<FutureState, T> consumeContext = context.CreateConsumeContext();

                var resultId = getResultId(consumeContext);

                return context.Instance.SetResult(consumeContext, resultId, x => Task.FromResult(messageFactory(x)));
            });
        }

        public static EventActivityBinder<FutureState, Fault<T>> SetFault<T>(this EventActivityBinder<FutureState, Fault<T>> binder,
            Func<ConsumeEventContext<FutureState, Fault<T>>, Guid> getResultId, AsyncEventMessageFactory<FutureState, Fault<T>, Fault<T>> messageFactory)
        {
            return binder.ThenAsync(context =>
            {
                ConsumeEventContext<FutureState, Fault<T>> consumeContext = context.CreateConsumeContext();

                var resultId = getResultId(consumeContext);

                return context.Instance.SetFault(consumeContext, resultId, messageFactory);
            });
        }

        public static EventActivityBinder<FutureState, T> SetFutureCompleted<T, TResult>(this EventActivityBinder<FutureState, T> binder,
            AsyncEventMessageFactory<FutureState, T, TResult> messageFactory)
            where T : class
            where TResult : class
        {
            return binder.ThenAsync(context => context.Instance.SetResult(context.CreateConsumeContext(), context.Instance.CorrelationId, messageFactory));
        }

        public static EventActivityBinder<FutureState, T> SetFutureFaulted<T, TFault>(this EventActivityBinder<FutureState, T> binder,
            AsyncEventMessageFactory<FutureState, T, TFault> messageFactory)
            where T : class
            where TFault : class
        {
            return binder.ThenAsync(context => context.Instance.SetFault(context.CreateConsumeContext(), context.Instance.CorrelationId, messageFactory));
        }

        public static EventActivityBinder<FutureState, T> RespondToSubscribers<T, TResponse>(this EventActivityBinder<FutureState, T> binder,
            AsyncEventMessageFactory<FutureState, TResponse> asyncMessageFactory)
            where T : class
            where TResponse : class
        {
            static async Task SendResponse(ConsumeContext context, TResponse response, Uri address, Guid? requestId)
            {
                var endpoint = await context.GetSendEndpoint(address).ConfigureAwait(false);

                await endpoint.Send(response, x => x.RequestId = requestId, context.CancellationToken).ConfigureAwait(false);
            }

            return binder.ThenAsync(async context =>
            {
                if (context.Instance.HasSubscriptions())
                {
                    HashSet<FutureSubscription> subscriptions = context.Instance.Subscriptions;

                    var consumeContext = new AutomatonymousConsumeEventContext<FutureState>(context, context.GetPayload<ConsumeContext>());

                    var response = await asyncMessageFactory(consumeContext).ConfigureAwait(false);

                    if (subscriptions != null)
                    {
                        await Task.WhenAll(subscriptions.Select(subscription => SendResponse(consumeContext, response, subscription.Address,
                            subscription.RequestId))).ConfigureAwait(false);
                    }
                }
            });
        }
    }
}
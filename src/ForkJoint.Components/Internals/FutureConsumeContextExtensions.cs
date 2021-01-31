namespace ForkJoint.Components.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Initializers;
    using Pipeline;


    public static class FutureConsumeContextExtensions
    {
        public static FutureConsumeContext<T> CreateFutureConsumeContext<T>(this BehaviorContext<FutureState> context, T message)
            where T : class
        {
            return new MassTransitFutureConsumeContext<T>(context, context.GetPayload<ConsumeContext>(), message);
        }

        public static FutureConsumeContext<T> CreateFutureConsumeContext<T>(this BehaviorContext<FutureState, T> context)
            where T : class
        {
            return new MassTransitFutureConsumeContext<T>(context, context.GetPayload<ConsumeContext>(), context.Data);
        }

        public static FutureConsumeContext CreateFutureConsumeContext(this BehaviorContext<FutureState> context)
        {
            return new MassTransitFutureConsumeContext(context, context.GetPayload<ConsumeContext>());
        }

        public static async Task SendMessageToSubscriptions<T>(this FutureConsumeContext context, IEnumerable<FutureSubscription> subscriptions,
            IMessageInitializer<T> initializer, InitializeContext<T> initializeContext, object values)
            where T : class
        {
            List<Task> tasks = subscriptions.Select(async sub =>
            {
                var endpoint = await context.GetSendEndpoint(sub.Address).ConfigureAwait(false);

                if (sub.RequestId.HasValue)
                {
                    var pipe = new FutureResponsePipe<T>(sub.RequestId.Value);

                    await initializer.Send(endpoint, initializeContext, values, pipe).ConfigureAwait(false);
                }
                else
                    await initializer.Send(endpoint, initializeContext, values).ConfigureAwait(false);
            }).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public static async Task SendMessageToSubscriptions<T>(this FutureConsumeContext context, IEnumerable<FutureSubscription> subscriptions, T message)
            where T : class
        {
            List<Task> tasks = subscriptions.Select(async sub =>
            {
                var endpoint = await context.GetSendEndpoint(sub.Address).ConfigureAwait(false);

                if (sub.RequestId.HasValue)
                {
                    var pipe = new FutureResponsePipe<T>(sub.RequestId.Value);

                    await endpoint.Send(message, pipe, context.CancellationToken).ConfigureAwait(false);
                }
                else
                    await endpoint.Send(message, context.CancellationToken).ConfigureAwait(false);
            }).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
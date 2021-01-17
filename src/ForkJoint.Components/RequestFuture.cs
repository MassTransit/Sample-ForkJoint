namespace ForkJoint.Components
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Util;


    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    /// <summary>
    /// RequestFuture is a durable future that sends a request to a consumer and completes or faults
    /// depending upon the response.
    /// The initiating event correlation must be declared
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TCompleted"></typeparam>
    public abstract class RequestFuture<TRequest, TCompleted, TCommand, TResult> :
        Future<TRequest, TCompleted>
        where TRequest : class
        where TCommand : class
        where TResult : class
        where TCompleted : class
    {
        protected RequestFuture()
        {
            Event(() => CommandCompleted, x =>
            {
                x.CorrelateById(context => context.RequestId ?? throw new RequestException("RequestId not present"));
                x.OnMissingInstance(m => m.Fault());
            });
            Event(() => CommandFaulted, x =>
            {
                x.CorrelateById(context => context.RequestId ?? throw new RequestException("RequestId not present"));
                x.OnMissingInstance(m => m.Fault());
            });

            Initially(
                When(FutureRequested)
                    .ThenAsync(async context =>
                    {
                        ConsumeEventContext<FutureState, TRequest> consumeContext = context.CreateConsumeContext();

                        var pipe = new ResponsePipe<TCommand>(consumeContext.ReceiveContext.InputAddress, context.Instance.CorrelationId);

                        await SendCommand(consumeContext, pipe).ConfigureAwait(false);
                    })
            );

            DuringAny(
                When(CommandCompleted)
                    .SetFutureCompleted(CreateCompleted)
                    .NotifySubscribers(x => GetCompleted(x))
                    .TransitionTo(Completed),
                When(CommandFaulted)
                    .SetFutureFaulted(CreateFaulted)
                    .NotifySubscribers(x => GetFaulted(x))
                    .TransitionTo(Faulted)
            );
        }

        public Event<TResult> CommandCompleted { get; protected set; }
        public Event<Fault<TCommand>> CommandFaulted { get; protected set; }

        /// <summary>
        /// Optional, sets the address of the request service. By default, requests are published.
        /// </summary>
        protected Uri DestinationAddress { get; set; }

        protected virtual async Task SendCommand(ConsumeEventContext<FutureState, TRequest> context, IPipe<SendContext<TCommand>> pipe)
        {
            var command = await CreateCommand(context).ConfigureAwait(false);

            if (DestinationAddress != null)
            {
                var endpoint = await context.GetSendEndpoint(DestinationAddress).ConfigureAwait(false);

                await endpoint.Send(command, pipe).ConfigureAwait(false);
            }
            else
                await context.Publish(command, pipe).ConfigureAwait(false);
        }

        protected virtual Task<TCompleted> CreateCompleted(ConsumeEventContext<FutureState, TResult> context)
        {
            return Init<TResult, TCompleted>(context);
        }

        protected virtual Task<Fault<TRequest>> CreateFaulted(ConsumeEventContext<FutureState, Fault<TCommand>> context)
        {
            var message = context.Instance.GetRequest<TRequest>();

            return context.Init<Fault<TRequest>>(new
            {
                context.Data.FaultId,
                context.Data.FaultedMessageId,
                context.Data.Timestamp,
                context.Data.Exceptions,
                context.Data.Host,
                context.Data.FaultMessageTypes,
                Message = message
            });
        }

        protected abstract Task<TCommand> CreateCommand(ConsumeEventContext<FutureState, TRequest> context);


        protected class ResponsePipe<T> :
            IPipe<SendContext<T>>
            where T : class
        {
            readonly Guid _requestId;
            readonly Uri _responseAddress;

            public ResponsePipe(Uri responseAddress, Guid requestId)
            {
                _responseAddress = responseAddress;
                _requestId = requestId;
            }

            public Task Send(SendContext<T> context)
            {
                context.RequestId = _requestId;
                context.ResponseAddress = _responseAddress;

                return TaskUtil.Completed;
            }

            public void Probe(ProbeContext context)
            {
                context.CreateFilterScope(nameof(ResponsePipe<T>));
            }
        }
    }
}
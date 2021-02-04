namespace ForkJoint.Components
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using Configurators;
    using Endpoints;
    using Internals;
    using MassTransit;


    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    /// <summary>
    /// RequestFuture is a durable future that sends a request to a consumer and completes or faults
    /// depending upon the response.
    /// The initiating event correlation must be declared
    /// </summary>
    public abstract class RequestFuture<TRequest, TResponse, TCommand, TResult> :
        Future<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
        where TCommand : class
        where TResult : class
    {
        readonly FutureCommand<TRequest, TCommand> _command = new();
        readonly FutureFault<TRequest, Fault<TRequest>, Fault<TCommand>> _fault = new();
        readonly FutureResponse<TRequest, TResult, TResponse> _response = new();

        protected RequestFuture()
        {
            Initially(
                When(FutureRequested)
                    .ThenAsync(context => SendCommand(context))
            );

            Event(() => CommandCompleted, x =>
            {
                x.CorrelateById(context => RequestIdOrFault(context));
                x.OnMissingInstance(m => m.Execute(context => throw new FutureNotFoundException(typeof(TRequest), context.RequestId ?? default)));
                x.ConfigureConsumeTopology = false;
            });

            Event(() => CommandFaulted, x =>
            {
                x.CorrelateById(context => RequestIdOrFault(context));
                x.OnMissingInstance(m => m.Execute(context => throw new FutureNotFoundException(typeof(TRequest), context.RequestId ?? default)));
                x.ConfigureConsumeTopology = false;
            });

            DuringAny(
                When(CommandCompleted)
                    .ThenAsync(context => SetCompleted(context))
                    .TransitionTo(Completed),
                When(CommandFaulted)
                    .ThenAsync(context => SetFaulted(context))
                    .TransitionTo(Faulted)
            );
        }

        public Event<TResult> CommandCompleted { get; protected set; }
        public Event<Fault<TCommand>> CommandFaulted { get; protected set; }

        protected void Command(Action<IFutureCommandConfigurator<TRequest, TCommand>> configure)
        {
            var configurator = new FutureCommandConfigurator<TRequest, TCommand>(_command);

            configure?.Invoke(configurator);
        }

        protected void Response(Action<IFutureResponseConfigurator<TResult, TResponse>> configure)
        {
            var configurator = new FutureResponseConfigurator<TRequest, TResult, TResponse>(_response);

            configure?.Invoke(configurator);
        }

        protected void Fault(Action<IFutureFaultConfigurator<Fault<TRequest>, Fault<TCommand>>> configure)
        {
            var configurator = new FutureFaultConfigurator<TRequest, Fault<TRequest>, Fault<TCommand>>(_fault);

            configure?.Invoke(configurator);
        }

        Task SendCommand(BehaviorContext<FutureState, TRequest> context)
        {
            FutureConsumeContext<TRequest> consumeContext = context.CreateFutureConsumeContext();

            return _command.SendCommand(consumeContext);
        }

        Task SetCompleted(BehaviorContext<FutureState, TResult> context)
        {
            FutureConsumeContext<TResult> consumeContext = context.CreateFutureConsumeContext();

            return _response.SendResponse(consumeContext, consumeContext.Instance.Subscriptions.ToArray());
        }

        Task SetFaulted(BehaviorContext<FutureState, Fault<TCommand>> context)
        {
            FutureConsumeContext<Fault<TCommand>> consumeContext = context.CreateFutureConsumeContext();

            return _fault.SendFault(consumeContext, consumeContext.Instance.Subscriptions.ToArray());
        }
    }


    public abstract class RequestFuture<TRequest, TCompleted> :
        RequestFuture<TRequest, TCompleted, TRequest, TCompleted>
        where TRequest : class
        where TCompleted : class
    {
    }
}
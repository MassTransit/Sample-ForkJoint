namespace ForkJoint.Api.Components.Futures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using ForkJoint.Components;
    using ForkJoint.Components.Configurators;
    using ForkJoint.Components.Endpoints;
    using ForkJoint.Components.Internals;
    using MassTransit;


    // ReSharper disable UnassignedGetOnlyAutoProperty
    // ReSharper disable MemberCanBePrivate.Global
    public abstract class OrderLineFuture<TRequest, TResponse, TFault> :
        Future<TRequest, TResponse, TFault>
        where TRequest : class
        where TResponse : class
        where TFault : class
    {
        readonly FutureFault<TFault> _fault = new();
        readonly FutureResponse<TRequest, TResponse> _response = new();

        protected OrderLineFuture()
        {
            Event(() => LineCompleted, x =>
            {
                x.CorrelateById(context => context.Message.OrderId);
                x.OnMissingInstance(m => m.Fault());
            });
            Event(() => LineFaulted, x =>
            {
                x.CorrelateById(context => context.Message.Message.OrderId);
                x.OnMissingInstance(m => m.Fault());
            });

            DuringAny(
                When(LineCompleted)
                    .SetResult(x => x.Message.OrderLineId, x => x.Message)
                    .IfElse(context => context.Instance.Completed.HasValue,
                        completed => completed
                            .ThenAsync(context => SetCompleted(context))
                            .TransitionTo(Completed),
                        notCompleted => notCompleted.If(context => context.Instance.Faulted.HasValue,
                            faulted => faulted
                                .ThenAsync(context => SetFaulted(context))
                                .TransitionTo(Faulted))),
                When(LineFaulted)
                    .SetFault(context => context.Message.Message.OrderLineId, x => x.Message)
                    .If(context => context.Instance.Faulted.HasValue,
                        faulted => faulted
                            .ThenAsync(context => SetFaulted(context))
                            .TransitionTo(Faulted))
            );

            Fault(x => x.Init(context =>
            {
                var message = context.Instance.GetRequest<TRequest>();

                // use supported message types to deserialize results...

                List<Fault> faults = context.Instance.Faults.Select(fault => fault.Value.ToObject<Fault>()).ToList();

                Fault faulted = faults.First();

                ExceptionInfo[] exceptions = faults.SelectMany(fault => fault.Exceptions).ToArray();

                return new
                {
                    faulted.FaultId,
                    faulted.FaultedMessageId,
                    Timestamp = context.Instance.Faulted,
                    Exceptions = exceptions,
                    faulted.Host,
                    faulted.FaultMessageTypes,
                    Message = message
                };
            }));
        }

        public Event<OrderLineCompleted> LineCompleted { get; protected set; }
        public Event<Fault<OrderLine>> LineFaulted { get; protected set; }

        protected void Response(Action<IFutureResponseConfigurator<TResponse>> configure)
        {
            var configurator = new FutureResponseConfigurator<TRequest, TResponse>(_response);

            configure?.Invoke(configurator);
        }

        protected void Fault(Action<IFutureFaultConfigurator<TFault>> configure)
        {
            var configurator = new FutureFaultConfigurator<TFault>(_fault);

            configure?.Invoke(configurator);
        }

        Task SetCompleted(BehaviorContext<FutureState> context)
        {
            var consumeContext = context.CreateFutureConsumeContext();

            return _response.SendResponse(consumeContext, consumeContext.Instance.Subscriptions.ToArray());
        }

        Task SetFaulted(BehaviorContext<FutureState> context)
        {
            var consumeContext = context.CreateFutureConsumeContext();

            return _fault.SendFault(consumeContext, consumeContext.Instance.Subscriptions.ToArray());
        }
    }
}
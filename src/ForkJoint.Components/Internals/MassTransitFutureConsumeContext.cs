namespace ForkJoint.Components.Internals
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Context;


    public class MassTransitFutureConsumeContext<TMessage> :
        ConsumeContextProxy,
        FutureConsumeContext<TMessage>
        where TMessage : class
    {
        readonly BehaviorContext<FutureState> _context;

        public MassTransitFutureConsumeContext(BehaviorContext<FutureState> context, ConsumeContext consumeContext, TMessage message)
            : base(consumeContext)
        {
            _context = context;
            Message = message;
        }

        public override bool HasPayloadType(Type contextType)
        {
            return _context.HasPayloadType(contextType) || base.HasPayloadType(contextType);
        }

        public override bool TryGetPayload<TPayload>(out TPayload context)
        {
            if (_context.TryGetPayload(out context))
                return true;

            return base.TryGetPayload(out context);
        }

        public override TPayload GetOrAddPayload<TPayload>(PayloadFactory<TPayload> payloadFactory)
        {
            if (_context.TryGetPayload(out TPayload payload))
                return payload;

            if (base.TryGetPayload(out payload))
                return payload;

            return _context.GetOrAddPayload(payloadFactory);
        }

        public override T AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory)
        {
            if (this is T context)
                return context;

            if (_context.TryGetPayload<T>(out var payload))
                return _context.AddOrUpdatePayload(addFactory, updateFactory);

            if (base.TryGetPayload(out payload))
            {
                T Add()
                {
                    return updateFactory(payload);
                }

                return _context.AddOrUpdatePayload(Add, updateFactory);
            }

            return _context.AddOrUpdatePayload(addFactory, updateFactory);
        }

        public FutureState Instance => _context.Instance;

        public Task Raise(Event @event)
        {
            return _context.Raise(@event);
        }

        public Task Raise<TData>(Event<TData> @event, TData data)
        {
            return _context.Raise(@event, data);
        }

        Event EventContext<FutureState>.Event => _context.Event;

        public Task NotifyConsumed(TimeSpan duration, string consumerType)
        {
            return NotifyConsumed(this, duration, consumerType);
        }

        public Task NotifyFaulted(TimeSpan duration, string consumerType, Exception exception)
        {
            return NotifyFaulted(this, duration, consumerType, exception);
        }

        public TMessage Message { get; }

        public Guid FutureId => Instance.CorrelationId;
    }
}
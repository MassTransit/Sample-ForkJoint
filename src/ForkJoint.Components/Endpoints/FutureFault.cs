namespace ForkJoint.Components.Endpoints
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GreenPipes;


    public class FutureFault<TRequest, TFault, TInput> :
        ISpecification
        where TRequest : class
        where TFault : class
        where TInput : class
    {
        static readonly Default _default = new Default();
        IFaultEndpoint<TInput> _endpoint;

        public FutureFault()
        {
            _endpoint = new InitializerFaultEndpoint<TRequest, TFault, TInput>(DefaultProvider);
        }

        public AsyncFutureMessageFactory<TInput, TFault> Factory
        {
            set => _endpoint = new FactoryFaultEndpoint<TInput, TFault>(value);
        }

        public InitializerValueProvider<TInput> Initializer
        {
            set => _endpoint = new InitializerFaultEndpoint<TRequest, TFault, TInput>(value);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_endpoint == null)
                yield return this.Failure("Fault", "Factory", "Init or Create must be configured");
        }

        static object DefaultProvider(FutureConsumeContext<TInput> context)
        {
            return _default;
        }

        public Task SendFault(FutureConsumeContext<TInput> context, params FutureSubscription[] subscriptions)
        {
            return context.Instance.HasSubscriptions()
                ? _endpoint.SendFault(context, subscriptions)
                : _endpoint.SendFault(context);
        }


        class Default
        {
        }
    }


    public class FutureFault<TFault> :
        ISpecification
        where TFault : class
    {
        static readonly Default _default = new Default();
        IFaultEndpoint _endpoint;

        public FutureFault()
        {
            _endpoint = new InitializerFaultEndpoint<TFault>(DefaultProvider);
        }

        public AsyncFutureMessageFactory<TFault> Factory
        {
            set => _endpoint = new FactoryFaultEndpoint<TFault>(value);
        }

        public InitializerValueProvider Initializer
        {
            set => _endpoint = new InitializerFaultEndpoint<TFault>(value);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_endpoint == null)
                yield return this.Failure("Fault", "Factory", "Init or Create must be configured");
        }

        static object DefaultProvider(FutureConsumeContext context)
        {
            return _default;
        }

        public Task SendFault(FutureConsumeContext context, params FutureSubscription[] subscriptions)
        {
            return context.Instance.HasSubscriptions()
                ? _endpoint.SendFault(context, subscriptions)
                : _endpoint.SendFault(context);
        }


        class Default
        {
        }
    }
}
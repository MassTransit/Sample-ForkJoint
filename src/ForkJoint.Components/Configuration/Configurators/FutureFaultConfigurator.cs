namespace ForkJoint.Components.Configurators
{
    using System;
    using Endpoints;


    public class FutureFaultConfigurator<TRequest, TFault, TInput> :
        IFutureFaultConfigurator<TFault, TInput>
        where TInput : class
        where TFault : class
        where TRequest : class
    {
        readonly FutureFault<TRequest, TFault, TInput> _fault;

        public FutureFaultConfigurator(FutureFault<TRequest, TFault, TInput> fault)
        {
            _fault = fault;
        }

        /// <summary>
        /// Sets the response object initializer, along with an object provider to initialize the message
        /// </summary>
        /// <param name="provider"></param>
        public void Init(InitializerValueProvider<TInput> provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _fault.Initializer = provider;
        }

        public void Create(AsyncFutureMessageFactory<TInput, TFault> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _fault.Factory = factory;
        }
    }


    public class FutureFaultConfigurator<TFault> :
        IFutureFaultConfigurator<TFault>
        where TFault : class
    {
        readonly FutureFault<TFault> _fault;

        public FutureFaultConfigurator(FutureFault<TFault> fault)
        {
            _fault = fault;
        }

        /// <summary>
        /// Sets the response object initializer, along with an object provider to initialize the message
        /// </summary>
        /// <param name="provider"></param>
        public void Init(InitializerValueProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _fault.Initializer = provider;
        }

        public void Create(AsyncFutureMessageFactory<TFault> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _fault.Factory = factory;
        }
    }
}
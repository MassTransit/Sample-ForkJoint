namespace ForkJoint.Components.Configurators
{
    using System;
    using Endpoints;


    public class FutureResponseConfigurator<TInput, TResponse> :
        IFutureResponseConfigurator<TInput, TResponse>
        where TInput : class
        where TResponse : class
    {
        readonly FutureResponse<TInput, TResponse> _response;

        public FutureResponseConfigurator(FutureResponse<TInput, TResponse> response)
        {
            _response = response;
        }

        /// <summary>
        /// Sets the response object initializer, along with an object provider to initialize the message
        /// </summary>
        /// <param name="provider"></param>
        public void Init(InitializerValueProvider<TInput> provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _response.Initializer = provider;
        }

        public void Create(FutureMessageFactory<TInput, TResponse> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _response.Factory = factory;
        }
    }
}
namespace ForkJoint.Components.Configurators
{
    using System;
    using Endpoints;


    public class FutureResponseConfigurator<TRequest, TInput, TResponse> :
        IFutureResponseConfigurator<TInput, TResponse>
        where TRequest : class
        where TInput : class
        where TResponse : class
    {
        readonly FutureResponse<TRequest, TInput, TResponse> _response;

        public FutureResponseConfigurator(FutureResponse<TRequest, TInput, TResponse> response)
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

        public void Create(AsyncFutureMessageFactory<TInput, TResponse> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _response.Factory = factory;
        }
    }


    public class FutureResponseConfigurator<TRequest, TResponse> :
        IFutureResponseConfigurator<TResponse>
        where TRequest : class
        where TResponse : class
    {
        readonly FutureResponse<TRequest, TResponse> _response;

        public FutureResponseConfigurator(FutureResponse<TRequest, TResponse> response)
        {
            _response = response;
        }

        /// <summary>
        /// Sets the response object initializer, along with an object provider to initialize the message
        /// </summary>
        /// <param name="provider"></param>
        public void Init(InitializerValueProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _response.Initializer = provider;
        }

        public void Create(AsyncFutureMessageFactory<TResponse> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _response.Factory = factory;
        }
    }
}
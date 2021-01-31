namespace ForkJoint.Components
{
    using System;
    using Automatonymous;


    public interface IFutureCommandConfigurator<out TRequest, TCommand>
        where TRequest : class
        where TCommand : class
    {
        /// <summary>
        /// Sets the command destination address
        /// </summary>
        Uri DestinationAddress { set; }

        /// <summary>
        /// Sets the destination address provider
        /// </summary>
        DestinationAddressProvider<FutureState> DestinationAddressProvider { set; }

        /// <summary>
        /// Adds an object initializer to the command, on top of the <see cref="FutureState" /> and <typeparamref name="TRequest" />
        /// which are added automatically.
        /// </summary>
        /// <param name="provider"></param>
        void Init(InitializerValueProvider<TRequest> provider);

        /// <summary>
        /// Replaces the command initializer with a custom message factory
        /// </summary>
        /// <param name="factory"></param>
        void Create(AsyncFutureMessageFactory<TRequest, TCommand> factory);
    }
}
namespace ForkJoint.Components
{
    public interface IFutureFaultConfigurator<TFault, out TInput>
        where TInput : class
        where TFault : class
    {
        /// <summary>
        /// Adds an object initializer to the command, on top of the <see cref="FutureState" /> and <typeparamref name="TInput" />
        /// which are added automatically.
        /// </summary>
        /// <param name="provider"></param>
        void Init(InitializerValueProvider<TInput> provider);

        /// <summary>
        /// Replaces the command initializer with a custom message factory
        /// </summary>
        /// <param name="factory"></param>
        void Create(AsyncFutureMessageFactory<TInput, TFault> factory);
    }


    public interface IFutureFaultConfigurator<TFault>
        where TFault : class
    {
        /// <summary>
        /// Adds an object initializer to the command, on top of the <see cref="FutureState" /> and <typeparamref name="TInput" />
        /// which are added automatically.
        /// </summary>
        /// <param name="provider"></param>
        void Init(InitializerValueProvider provider);

        /// <summary>
        /// Replaces the command initializer with a custom message factory
        /// </summary>
        /// <param name="factory"></param>
        void Create(AsyncFutureMessageFactory<TFault> factory);
    }
}
namespace ForkJoint.Components
{
    public interface IFutureResponseConfigurator<out TResult, TResponse>
        where TResult : class
        where TResponse : class
    {
        /// <summary>
        /// Adds an object initializer to the command, on top of the <see cref="FutureState" /> and <typeparamref name="TResult" />
        /// which are added automatically.
        /// </summary>
        /// <param name="provider"></param>
        void Init(InitializerValueProvider<TResult> provider);

        /// <summary>
        /// Replaces the command initializer with a custom message factory
        /// </summary>
        /// <param name="factory"></param>
        void Create(AsyncFutureMessageFactory<TResult, TResponse> factory);
    }


    public interface IFutureResponseConfigurator<TResponse>
        where TResponse : class
    {
        /// <summary>
        /// Adds an object initializer to the command, on top of the <see cref="FutureState" /> which is added automatically.
        /// </summary>
        /// <param name="provider"></param>
        void Init(InitializerValueProvider provider);

        /// <summary>
        /// Replaces the command initializer with a custom message factory
        /// </summary>
        /// <param name="factory"></param>
        void Create(AsyncFutureMessageFactory<TResponse> factory);
    }
}
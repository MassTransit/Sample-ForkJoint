namespace ForkJoint.Components
{
    using System.Threading.Tasks;


    /// <summary>
    /// Given the event context and request, returns an object used to complete the initialization of the object type
    /// </summary>
    /// <param name="context"></param>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="T"></typeparam>
    public delegate Task<T> AsyncFutureMessageFactory<in TMessage, T>(FutureConsumeContext<TMessage> context)
        where TMessage : class
        where T : class;


    /// <summary>
    /// Given the event context and request, returns an object used to complete the initialization of the object type
    /// </summary>
    /// <param name="context"></param>
    /// <typeparam name="T"></typeparam>
    public delegate Task<T> AsyncFutureMessageFactory<T>(FutureConsumeContext context)
        where T : class;
}
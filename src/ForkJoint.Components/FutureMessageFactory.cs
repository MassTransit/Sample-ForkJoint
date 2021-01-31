namespace ForkJoint.Components
{
    using System.Threading.Tasks;


    /// <summary>
    /// Given the event context and request, returns an object used to complete the initialization of the object type
    /// </summary>
    /// <param name="context"></param>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="T"></typeparam>
    public delegate Task<T> FutureMessageFactory<in TData, T>(FutureConsumeContext<TData> context)
        where TData : class
        where T : class;
}
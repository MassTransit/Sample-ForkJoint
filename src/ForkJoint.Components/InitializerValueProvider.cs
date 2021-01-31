namespace ForkJoint.Components
{
    /// <summary>
    /// Given the event context and request, returns an object used to complete the initialization of the object type
    /// </summary>
    /// <param name="context"></param>
    /// <typeparam name="TRequest"></typeparam>
    public delegate object InitializerValueProvider<in TRequest>(FutureConsumeContext<TRequest> context)
        where TRequest : class;
}
namespace ForkJoint.Components.Contracts
{
    public interface Request<out TRequest>
        where TRequest : class
    {
        TRequest Request { get; }
    }
}
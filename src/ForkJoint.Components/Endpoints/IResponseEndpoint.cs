namespace ForkJoint.Components.Endpoints
{
    using System.Threading.Tasks;


    public interface IResponseEndpoint<in TResult>
        where TResult : class
    {
        Task SendResponse(FutureConsumeContext<TResult> context, params FutureSubscription[] subscriptions);
    }


    public interface IResponseEndpoint
    {
        Task SendResponse(FutureConsumeContext context, params FutureSubscription[] subscriptions);
    }
}
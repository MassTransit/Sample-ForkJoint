namespace ForkJoint.Components.Endpoints
{
    using System.Threading.Tasks;
    using Automatonymous;


    public interface ICommandEndpoint<in TRequest, out TCommand>
        where TRequest : class
        where TCommand : class
    {
        DestinationAddressProvider<FutureState> DestinationAddressProvider { set; }

        Task SendCommand(FutureConsumeContext<TRequest> context);
    }
}
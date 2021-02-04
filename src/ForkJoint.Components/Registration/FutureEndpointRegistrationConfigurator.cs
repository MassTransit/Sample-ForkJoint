namespace ForkJoint.Components.Registration
{
    using MassTransit.Registration;


    public class FutureEndpointRegistrationConfigurator<TFuture> :
        EndpointRegistrationConfigurator<TFuture>,
        IFutureEndpointRegistrationConfigurator<TFuture>
        where TFuture : class
    {
    }
}
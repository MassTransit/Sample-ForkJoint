namespace ForkJoint.Components.Registration
{
    using MassTransit.Registration;


    public interface IFutureEndpointRegistrationConfigurator<TFuture> :
        IEndpointRegistrationConfigurator
        where TFuture : class
    {
    }
}
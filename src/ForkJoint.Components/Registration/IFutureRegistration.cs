namespace ForkJoint.Components.Registration
{
    using System;
    using Automatonymous;
    using MassTransit;
    using MassTransit.Registration;


    public interface IFutureRegistration<TFuture> :
        IFutureRegistration
        where TFuture : MassTransitStateMachine<FutureState>
    {
    }


    public interface IFutureRegistration
    {
        Type FutureType { get; }

        void Configure(IReceiveEndpointConfigurator configurator, IConfigurationServiceProvider repositoryFactory);

        IFutureDefinition GetDefinition(IConfigurationServiceProvider provider);
    }
}
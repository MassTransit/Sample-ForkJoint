namespace ForkJoint.Api;

using MassTransit;


public static class CustomConfigurationExtensions
{
    /// <summary>
    /// Should be using on every UsingRabbitMq configuration
    /// </summary>
    /// <param name="configurator"></param>
    public static void ApplyCustomBusConfiguration(this IBusFactoryConfigurator configurator)
    {
        var entityNameFormatter = configurator.MessageTopology.EntityNameFormatter;

        configurator.MessageTopology.SetEntityNameFormatter(new CustomEntityNameFormatter(entityNameFormatter));
    }

    /// <summary>
    /// Should be used on every AddMassTransit configuration
    /// </summary>
    /// <param name="configurator"></param>
    public static void ApplyCustomMassTransitConfiguration(this IBusRegistrationConfigurator configurator)
    {
        configurator.SetEndpointNameFormatter(new CustomEndpointNameFormatter());
    }
}
namespace ForkJoint.Contracts
{
    using MassTransit;


    public static class EntityNameFormatterConfigurationExtensions
    {
        public static void SetCustomEntityNameFormatter(this IBusFactoryConfigurator configurator)
        {
            var entityNameFormatter = configurator.MessageTopology.EntityNameFormatter;

            configurator.MessageTopology.SetEntityNameFormatter(new CustomEntityNameFormatter(entityNameFormatter));
        }
    }
}
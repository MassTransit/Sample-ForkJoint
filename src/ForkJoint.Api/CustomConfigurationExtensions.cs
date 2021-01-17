namespace ForkJoint.Api
{
    using Automatonymous;
    using ForkJoint.Components;
    using GreenPipes;
    using MassTransit;
    using MassTransit.RabbitMqTransport;


    public static class CustomConfigurationExtensions
    {
        /// <summary>
        /// Configure future endpoints to have redelivery, retry, outbox, etc.
        /// </summary>
        /// <param name="configurator"></param>
        public static void ApplyFutureEndpointConfiguration<T>(this IReceiveEndpointConfigurator configurator)
            where T : class
        {
            if (configurator is IRabbitMqReceiveEndpointConfigurator rabbit)
            {
                rabbit.ConfigureConsumeTopology = false;
                rabbit.Bind<T>();
            }

            configurator.UseScheduledRedelivery(r => r.Intervals(5000));
            configurator.UseMessageRetry(r => r.Intervals(100, 200, 500));
            configurator.UseInMemoryOutbox();
        }

        public static void FutureEndpoint<TFuture, TRequest>(this IBusFactoryConfigurator configurator, IBusRegistrationContext context)
            where TFuture : class, SagaStateMachine<FutureState>, new()
            where TRequest : class
        {
            var endpointNameFormatter = context.GetRequiredService<IEndpointNameFormatter>();

            configurator.ReceiveEndpoint(endpointNameFormatter.Message<TFuture>(), endpoint =>
            {
                endpoint.ApplyFutureEndpointConfiguration<TRequest>();

                endpoint.StateMachineSaga(new TFuture(), context);
            });
        }
    }
}
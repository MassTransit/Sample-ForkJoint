namespace ForkJoint.Tests
{
    using System;
    using Automatonymous;
    using Components;
    using GreenPipes;
    using MassTransit;
    using Microsoft.Extensions.DependencyInjection;


    public static class CustomConfigurationExtensions
    {
        /// <summary>
        /// Configure future endpoints to have redelivery, retry, outbox, etc.
        /// </summary>
        /// <param name="configurator"></param>
        public static void ApplyFutureEndpointConfiguration<T>(this IReceiveEndpointConfigurator configurator)
            where T : class
        {
            configurator.UseScheduledRedelivery(r => r.Intervals(5000));
            configurator.UseMessageRetry(r => r.Intervals(100, 200, 500));
            configurator.UseInMemoryOutbox();
        }

        public static void FutureEndpoint<TFuture, TRequest>(this IBusFactoryConfigurator configurator, IServiceProvider provider)
            where TFuture : class, SagaStateMachine<FutureState>, new()
            where TRequest : class
        {
            var endpointNameFormatter = provider.GetRequiredService<IEndpointNameFormatter>();

            configurator.ReceiveEndpoint(endpointNameFormatter.Message<TFuture>(), endpoint =>
            {
                endpoint.ApplyFutureEndpointConfiguration<TRequest>();

                endpoint.StateMachineSaga(new TFuture(), provider);
            });
        }
    }
}
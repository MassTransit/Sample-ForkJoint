namespace ForkJoint.Tests
{
    using System;
    using System.Threading.Tasks;
    using MassTransit;
    using MassTransit.Context;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using MassTransit.Futures;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;

    public class FutureTestFixture
    {
        protected ServiceProvider Provider;
        protected InMemoryTestHarness TestHarness;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var collection = new ServiceCollection()
                .AddSingleton<ILoggerFactory>(_ => new TestOutputLoggerFactory(true))
                .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
                .AddMassTransitInMemoryTestHarness(cfg =>
                {
                    cfg.AddSagaRepository<FutureState>()
                        .InMemoryRepository();

                    cfg.SetKebabCaseEndpointNameFormatter();

                    ConfigureMassTransit(cfg);
                })
                .AddGenericRequestClient();

            ConfigureServices(collection);

            Provider = collection.BuildServiceProvider(true);

            ConfigureLogging();

            TestHarness = Provider.GetRequiredService<InMemoryTestHarness>();
            TestHarness.TestTimeout = TimeSpan.FromSeconds(10);
            TestHarness.OnConfigureInMemoryBus += configurator =>
            {
                ConfigureInMemoryBus(configurator);
            };

            await TestHarness.Start();
        }

        protected virtual void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator)
        {
        }

        protected virtual void ConfigureServices(IServiceCollection collection)
        {
        }

        protected virtual void ConfigureInMemoryBus(IInMemoryBusFactoryConfigurator configurator)
        {
        }

        [OneTimeTearDown]
        public async Task Teardown()
        {
            try
            {
                await TestHarness.Stop();
            }
            finally
            {
                await Provider.DisposeAsync();
            }
        }

        void ConfigureLogging()
        {
            var loggerFactory = Provider.GetRequiredService<ILoggerFactory>();

            LogContext.ConfigureCurrentLogContext(loggerFactory);
        }
    }
}

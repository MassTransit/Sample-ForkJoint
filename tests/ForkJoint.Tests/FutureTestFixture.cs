namespace ForkJoint.Tests
{
    using System;
    using System.Threading.Tasks;
    using MassTransit;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;


    public class FutureTestFixture
    {
        protected ServiceProvider Provider;
        protected ITestHarness TestHarness;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var collection = new ServiceCollection()
                .AddSingleton<ILoggerFactory>(_ => new TestOutputLoggerFactory(true))
                .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddSagaRepository<FutureState>()
                        .InMemoryRepository();

                    cfg.SetKebabCaseEndpointNameFormatter();

                    ConfigureMassTransit(cfg);
                });

            ConfigureServices(collection);

            Provider = collection.BuildServiceProvider(true);

            ConfigureLogging();

            TestHarness = Provider.GetTestHarness();
            TestHarness.TestTimeout = TimeSpan.FromSeconds(10);

            await TestHarness.Start();
        }

        protected virtual void ConfigureMassTransit(IBusRegistrationConfigurator configurator)
        {
        }

        protected virtual void ConfigureServices(IServiceCollection collection)
        {
        }

        [OneTimeTearDown]
        public async Task Teardown()
        {
            await Provider.DisposeAsync();
        }

        void ConfigureLogging()
        {
            var loggerFactory = Provider.GetRequiredService<ILoggerFactory>();

            LogContext.ConfigureCurrentLogContext(loggerFactory);
        }
    }
}
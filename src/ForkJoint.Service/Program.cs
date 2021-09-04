namespace ForkJoint.Service
{
    using ForkJoint.Application;
    using ForkJoint.Application.Components;
    using ForkJoint.Application.Components.Activities;
    using ForkJoint.Application.Components.Consumers;
    using ForkJoint.Application.Components.Futures;
    using ForkJoint.Application.Components.ItineraryPlanners;
    using ForkJoint.Application.Services;
    using ForkJoint.Contracts;
    using MassTransit;
    using MassTransit.EntityFrameworkCoreIntegration;
    using MassTransit.Futures;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Reflection;

    public class Program
    {
        static bool? _isRunningInContainer;

        private static DependencyTrackingTelemetryModule dependencyTrackingTelemetryModule;
        private static TelemetryClient telemetryClient;

        static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.TryAddScoped<IItineraryPlanner<OrderBurger>, BurgerItineraryPlanner>();
                    services.TryAddSingleton<IGrill, Grill>();
                    services.TryAddSingleton<IFryer, Fryer>();
                    services.TryAddSingleton<IShakeMachine, ShakeMachine>();

                    AddTelemetry(hostContext, services);

                    services.AddDbContext<ForkJointSagaDbContext>(builder =>
                        builder.UseSqlServer(GetConnectionString(hostContext), m =>
                        {
                            m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                            m.MigrationsHistoryTable($"__{nameof(ForkJointSagaDbContext)}");
                        }));

                    services.AddDbContext<ApplicationOptimisticFutureSagaDbContext>(builder =>
                        builder.UseSqlServer(GetConnectionString(hostContext), m =>
                        {
                            m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                            m.MigrationsHistoryTable($"__{nameof(ApplicationOptimisticFutureSagaDbContext)}");
                        }));

                    services.AddGenericRequestClient();

                    services.AddMassTransit(x =>
                    {
                        x.ApplyCustomMassTransitConfiguration();

                        x.AddDelayedMessageScheduler();

                        x.SetEntityFrameworkSagaRepositoryProvider(r =>
                        {
                            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

                            r.LockStatementProvider = new SqlServerLockStatementProvider();

                            r.ExistingDbContext<ForkJointSagaDbContext>();
                        });

                        x.AddConsumersFromNamespaceContaining<CookOnionRingsConsumer>();

                        x.AddActivitiesFromNamespaceContaining<GrillBurgerActivity>();

                        x.AddFuturesFromNamespaceContaining<OrderFuture>();

                        x.AddSagaRepository<FutureState>()
                            .EntityFrameworkRepository(r =>
                            {
                                r.ConcurrencyMode = ConcurrencyMode.Optimistic;

                                //r.LockStatementProvider = new SqlServerLockStatementProvider();

                                r.ExistingDbContext<ApplicationOptimisticFutureSagaDbContext>();
                            });

                        x.AddSagaStateMachine<OptimisticConcurrencyTestsStateMachine, OptimisticConcurrencyTestsState>();

                        x.AddSagaRepository<OptimisticConcurrencyTestsState>()
                            .EntityFrameworkRepository(r =>
                            {
                                r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

                                r.LockStatementProvider = new SqlServerLockStatementProvider();

                                r.ExistingDbContext<ForkJointSagaDbContext>();
                            });

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.AutoStart = true;

                            cfg.ApplyCustomBusConfiguration();

                            if (IsRunningInContainer)
                                cfg.Host("rabbitmq");

                            cfg.UseDelayedMessageScheduler();

                            cfg.ConfigureEndpoints(context);
                        });
                    });

                    services.AddMassTransitHostedService();
                })
                .UseSerilog();

        private static void AddTelemetry(HostBuilderContext hostBuilderContext, IServiceCollection services)
        {
            var instrumentationKey = hostBuilderContext.Configuration.GetSection("ApplicationInsights:InstrumentationKey").Value;

            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                dependencyTrackingTelemetryModule = new DependencyTrackingTelemetryModule();

                dependencyTrackingTelemetryModule.IncludeDiagnosticSourceActivities.Add("MassTransit");

                TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();

                configuration.TelemetryProcessorChainBuilder.Use(next => new NoSqlTelemetryProcessor(next));

                configuration.InstrumentationKey = instrumentationKey;
                configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());

                telemetryClient = new TelemetryClient(configuration);

                dependencyTrackingTelemetryModule.Initialize(configuration);
            }

            //services.AddApplicationInsightsTelemetry(options =>
            //{
            //    options.EnableDependencyTrackingTelemetryModule = true;
            //});
            //services.AddApplicationInsightsTelemetryProcessor<NoSqlTelemetryProcessor>();

            //services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            //{
            //    module.IncludeDiagnosticSourceActivities.Add("MassTransit");
            //});
            //services.AddSingleton<ITelemetryInitializer, HttpDependenciesParsingTelemetryInitializer>();
        }

        static string GetConnectionString(HostBuilderContext hostBuilderContext)
        {
            var connectionString = hostBuilderContext.Configuration.GetConnectionString("ForkJoint");

            if (IsRunningInContainer)
                connectionString = connectionString.Replace("localhost", "mssql");

            return connectionString;
        }
    }
}

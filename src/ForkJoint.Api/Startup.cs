namespace ForkJoint.Api
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Components;
    using Components.Activities;
    using Components.Consumers;
    using Components.Futures;
    using Components.ItineraryPlanners;
    using Contracts;
    using MassTransit;
    using MassTransit.EntityFrameworkCoreIntegration;
    using MassTransit.Futures;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Services;


    public class Startup
    {
        static bool? _isRunningInContainer;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        IConfiguration Configuration { get; }

        static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;

        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddScoped<IItineraryPlanner<OrderBurger>, BurgerItineraryPlanner>();
            services.TryAddSingleton<IGrill, Grill>();
            services.TryAddSingleton<IFryer, Fryer>();
            services.TryAddSingleton<IShakeMachine, ShakeMachine>();

            services.AddApplicationInsightsTelemetry(options =>
            {
                options.EnableDependencyTrackingTelemetryModule = true;
            });
            services.AddApplicationInsightsTelemetryProcessor<NoSqlTelemetryProcessor>();

            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.IncludeDiagnosticSourceActivities.Add("MassTransit");
            });
            services.AddSingleton<ITelemetryInitializer, HttpDependenciesParsingTelemetryInitializer>();

            services.AddDbContext<ForkJointSagaDbContext>(builder =>
                builder.UseSqlServer(GetConnectionString(), m =>
                {
                    m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                    m.MigrationsHistoryTable($"__{nameof(ForkJointSagaDbContext)}");
                }));

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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ForkJoint.Api",
                    Version = "v1"
                });
            });
        }

        string GetConnectionString()
        {
            var connectionString = Configuration.GetConnectionString("ForkJoint");

            if (IsRunningInContainer)
                connectionString = connectionString.Replace("localhost", "mssql");

            return connectionString;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ForkJoint.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready"),
                    ResponseWriter = HealthCheckResponseWriter
                });

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions {ResponseWriter = HealthCheckResponseWriter});
            });
        }

        static Task HealthCheckResponseWriter(HttpContext context, HealthReport result)
        {
            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(entry => new JProperty(entry.Key, new JObject(
                    new JProperty("status", entry.Value.Status.ToString()),
                    new JProperty("description", entry.Value.Description),
                    new JProperty("data", JObject.FromObject(entry.Value.Data))))))));

            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(json.ToString(Formatting.Indented));
        }
    }
}
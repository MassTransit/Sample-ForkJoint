namespace ForkJoint.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Automatonymous;
    using MassTransit;
    using MassTransit.Definition;
    using MassTransit.Registration;
    using Microsoft.Extensions.DependencyInjection;
    using Registration;


    public static class FutureRegistrationExtensions
    {
        public static void AddFuture<TFuture, TDefinition>(this IServiceCollection services)
            where TFuture : MassTransitStateMachine<FutureState>, new()
            where TDefinition : class, IFutureDefinition<TFuture>
        {
            services.AddTransient<IFutureRegistration, FutureRegistration<TFuture>>();
            services.AddSingleton<TFuture>();
            services.AddTransient<IFutureDefinition<TFuture>, TDefinition>();
        }

        public static void AddFuture<TFuture>(this IServiceCollection services)
            where TFuture : MassTransitStateMachine<FutureState>, new()
        {
            services.AddTransient<IFutureRegistration<TFuture>, FutureRegistration<TFuture>>();
            services.AddTransient<IFutureRegistration>(provider => provider.GetService<IFutureRegistration<TFuture>>());
            services.AddSingleton<TFuture>();
        }

        public static void ConfigureFutureEndpoints(this IBusFactoryConfigurator configurator, IServiceProvider provider)
        {
            var endpointNameFormatter = provider.GetService<IEndpointNameFormatter>();
            endpointNameFormatter ??= DefaultEndpointNameFormatter.Instance;

            var configurationServiceProvider = provider.GetRequiredService<IConfigurationServiceProvider>();

            var registrations = provider.GetService<IEnumerable<IFutureRegistration>>();
            if (registrations != null)
            {
                List<IGrouping<string, IFutureDefinition>> futuresByEndpoint = registrations
                    .Select(x => x.GetDefinition(configurationServiceProvider))
                    .GroupBy(x => x.GetEndpointName(endpointNameFormatter))
                    .ToList();

                IEnumerable<string> endpointNames = futuresByEndpoint.Select(x => x.Key);

                var endpoints =
                    from e in endpointNames
                    join f in futuresByEndpoint on e equals f.Key into cs
                    from f in cs.DefaultIfEmpty()
                    where f.Any()
                    select new
                    {
                        Name = e,
                        Futures = f,
                        Definition = f?.Select(x => (IEndpointDefinition)new DelegateEndpointDefinition(e, x, x.EndpointDefinition)).Combine()
                            ?? new NamedEndpointDefinition(e)
                    };

                foreach (var endpoint in endpoints)
                {
                    configurator.ReceiveEndpoint(endpoint.Definition, endpointNameFormatter, cfg =>
                    {
                        foreach (var future in endpoint.Futures)
                        {
                            var registration = (IFutureRegistration)provider.GetService(typeof(IFutureRegistration<>).MakeGenericType(future.FutureType));

                            registration?.Configure(cfg, configurationServiceProvider);
                        }
                    });
                }
            }
        }
    }
}
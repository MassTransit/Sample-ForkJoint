namespace ForkJoint.Tests
{
    using System.Threading.Tasks;
    using Api.Components.Activities;
    using Api.Components.Futures;
    using Api.Components.ItineraryPlanners;
    using Api.Services;
    using Components;
    using Contracts;
    using MassTransit;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;


    [TestFixture]
    public class BurgerFuture_Specs :
        FutureTestFixture
    {
        [Test]
        public async Task Should_complete()
        {
            var orderId = NewId.NextGuid();
            var orderLineId = NewId.NextGuid();

            var scope = Provider.CreateScope();

            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<OrderBurger>>();

            Response<BurgerCompleted> response = await client.GetResponse<BurgerCompleted>(new
            {
                OrderId = orderId,
                Burger = new Burger
                {
                    BurgerId = orderLineId,
                    Weight = 1.0m,
                    Cheese = true,
                }
            });
        }

        protected override void ConfigureServices(IServiceCollection collection)
        {
            collection.AddSingleton<IGrill, Grill>();
            collection.AddScoped<IItineraryPlanner<OrderBurger>, BurgerItineraryPlanner>();
        }

        protected override void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator)
        {
            configurator.AddActivitiesFromNamespaceContaining<GrillBurgerActivity>();

            configurator.AddRequestClient<OrderBurger>();
            configurator.AddRequestClient<OrderOnionRings>();
        }

        protected override void ConfigureInMemoryBus(IInMemoryBusFactoryConfigurator configurator)
        {
            configurator.FutureEndpoint<BurgerFuture, OrderBurger>(Provider);
            configurator.FutureEndpoint<OnionRingsFuture, OrderOnionRings>(Provider);
        }
    }
}
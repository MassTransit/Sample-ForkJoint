namespace ForkJoint.Tests
{
    using System.Threading.Tasks;
    using Api.Components.Activities;
    using Api.Components.Consumers;
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
    public class OrderFuture_Specs :
        FutureTestFixture
    {
        [Test]
        public async Task Should_complete()
        {
            var orderId = NewId.NextGuid();
            var fryId = NewId.NextGuid();
            var burgerId = NewId.NextGuid();

            using var scope = Provider.CreateScope();

            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<SubmitOrder>>();

            Response<OrderCompleted, OrderFaulted> response = await client.GetResponse<OrderCompleted, OrderFaulted>(new
            {
                OrderId = orderId,
                Fries = new[]
                {
                    new
                    {
                        FryId = fryId,
                        Size = Size.Large
                    }
                },
                Burgers = new[]
                {
                    new Burger
                    {
                        BurgerId = burgerId,
                        Weight = 1.0m,
                        Cheese = true,
                    }
                },
                Shakes = default(Shake[]),
                FryShakes = default(FryShake[])
            });

            Assert.That(response.Is(out Response<OrderCompleted> completed), "Order did not complete");
        }

        [Test]
        public async Task Should_complete_durably()
        {
            var orderId = NewId.NextGuid();
            var fryId = NewId.NextGuid();
            var burgerId = NewId.NextGuid();

            using var scope = Provider.CreateScope();

            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<SubmitOrder>>();

            var values = new
            {
                OrderId = orderId,
                Fries = new[]
                {
                    new
                    {
                        FryId = fryId,
                        Size = Size.Large
                    }
                },
                Burgers = new[]
                {
                    new Burger
                    {
                        BurgerId = burgerId,
                        Weight = 1.0m,
                        Cheese = true,
                    }
                },
                Shakes = default(Shake[]),
                FryShakes = default(FryShake[])
            };

            Response<OrderCompleted, OrderFaulted> response = await client.GetResponse<OrderCompleted, OrderFaulted>(values);

            Assert.That(response.Is(out Response<OrderCompleted> _), "Order did not complete");

            response = await client.GetResponse<OrderCompleted, OrderFaulted>(values);

            Assert.That(response.Is(out Response<OrderCompleted> _), "Order did not complete from future");
        }

        [Test]
        public async Task Should_fault_with_lettuce()
        {
            var orderId = NewId.NextGuid();
            var fryId = NewId.NextGuid();
            var burgerId = NewId.NextGuid();

            using var scope = Provider.CreateScope();

            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<SubmitOrder>>();

            Response<OrderCompleted, OrderFaulted> response = await client.GetResponse<OrderCompleted, OrderFaulted>(new
            {
                OrderId = orderId,
                Fries = new[]
                {
                    new
                    {
                        FryId = fryId,
                        Size = Size.Large
                    }
                },
                Burgers = new[]
                {
                    new Burger
                    {
                        BurgerId = burgerId,
                        Weight = 1.0m,
                        Lettuce = true
                    }
                },
                Shakes = default(Shake[]),
                FryShakes = default(FryShake[])
            });

            Assert.That(response.Is(out Response<OrderFaulted> faulted), "Order should have faulted");
        }

        [Test]
        public async Task Should_fault_with_lettuce_durably()
        {
            var orderId = NewId.NextGuid();
            var fryId = NewId.NextGuid();
            var burgerId = NewId.NextGuid();

            using var scope = Provider.CreateScope();

            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<SubmitOrder>>();

            var values = new
            {
                OrderId = orderId,
                Fries = new[]
                {
                    new
                    {
                        FryId = fryId,
                        Size = Size.Large
                    }
                },
                Burgers = new[]
                {
                    new Burger
                    {
                        BurgerId = burgerId,
                        Weight = 1.0m,
                        Lettuce = true
                    }
                },
                Shakes = default(Shake[]),
                FryShakes = default(FryShake[])
            };

            Response<OrderCompleted, OrderFaulted> response = await client.GetResponse<OrderCompleted, OrderFaulted>(values);

            Assert.That(response.Is(out Response<OrderFaulted> _), "Order should have faulted");

            response = await client.GetResponse<OrderCompleted, OrderFaulted>(values);

            Assert.That(response.Is(out Response<OrderFaulted> _), "Order should have faulted durably");
        }

        protected override void ConfigureServices(IServiceCollection collection)
        {
            collection.AddSingleton<IGrill, Grill>();
            collection.AddScoped<IItineraryPlanner<OrderBurger>, BurgerItineraryPlanner>();
            collection.AddSingleton<IFryer, Fryer>();
        }

        protected override void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator)
        {
            configurator.AddConsumer<CookFryConsumer>();
            configurator.AddConsumer<CookOnionRingsConsumer>();
            configurator.AddActivitiesFromNamespaceContaining<GrillBurgerActivity>();

            configurator.AddRequestClient<SubmitOrder>();
            configurator.AddRequestClient<OrderOnionRings>();
        }

        protected override void ConfigureInMemoryBus(IInMemoryBusFactoryConfigurator configurator)
        {
            configurator.FutureEndpoint<BurgerFuture, OrderBurger>(Provider);
            configurator.FutureEndpoint<FryFuture, OrderFry>(Provider);
            configurator.FutureEndpoint<ShakeFuture, OrderShake>(Provider);
            configurator.FutureEndpoint<FryShakeFuture, OrderFryShake>(Provider);
            configurator.FutureEndpoint<OnionRingsFuture, OrderOnionRings>(Provider);
            configurator.FutureEndpoint<OrderFuture, SubmitOrder>(Provider);
        }
    }
}
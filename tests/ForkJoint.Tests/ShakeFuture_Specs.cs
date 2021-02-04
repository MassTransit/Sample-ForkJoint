namespace ForkJoint.Tests
{
    using System;
    using System.Threading.Tasks;
    using Api.Components.Consumers;
    using Api.Components.Futures;
    using Api.Services;
    using Components;
    using Contracts;
    using MassTransit;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;


    [TestFixture]
    public class ShakeFuture_Specs :
        FutureTestFixture
    {
        [Test]
        public async Task Should_complete()
        {
            var orderId = NewId.NextGuid();
            var orderLineId = NewId.NextGuid();

            var startedAt = DateTime.UtcNow;

            var scope = Provider.CreateScope();

            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<OrderShake>>();

            Response<ShakeCompleted> response = await client.GetResponse<ShakeCompleted>(new
            {
                OrderId = orderId,
                OrderLineId = orderLineId,
                Flavor = "Chocolate",
                Size = Size.Medium
            });

            Assert.That(response.Message.OrderId, Is.EqualTo(orderId));
            Assert.That(response.Message.OrderLineId, Is.EqualTo(orderLineId));
            Assert.That(response.Message.Size, Is.EqualTo(Size.Medium));
            Assert.That(response.Message.Created, Is.GreaterThan(startedAt));
            Assert.That(response.Message.Completed, Is.GreaterThan(response.Message.Created));
        }

        protected override void ConfigureServices(IServiceCollection collection)
        {
            collection.AddSingleton<IShakeMachine, ShakeMachine>();

            collection.AddFuture<ShakeFuture>();
        }

        protected override void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator)
        {
            configurator.AddConsumer<PourShakeConsumer>();

            configurator.AddRequestClient<OrderShake>();
        }
    }
}
namespace ForkJoint.Tests;

using System;
using System.Threading.Tasks;
using Api.Components.Consumers;
using Api.Components.Futures;
using Api.Services;
using Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;


[TestFixture]
public class FryShakeFuture_Specs :
    FutureTestFixture
{
    [Test]
    public async Task Should_complete()
    {
        var orderId = NewId.NextGuid();
        var orderLineId = NewId.NextGuid();

        var startedAt = DateTime.UtcNow;

        IRequestClient<OrderFryShake> client = TestHarness.GetRequestClient<OrderFryShake>();

        Response<FryShakeCompleted> response = await client.GetResponse<FryShakeCompleted>(new
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
        Assert.That(response.Message.Description, Contains.Substring("FryShake(2)"));
    }

    protected override void ConfigureServices(IServiceCollection collection)
    {
        collection.AddSingleton<IShakeMachine, ShakeMachine>();
        collection.AddSingleton<IFryer, Fryer>();
    }

    protected override void ConfigureMassTransit(IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<PourShakeConsumer>();
        configurator.AddConsumer<CookFryConsumer>();

        configurator.AddFuture<FryFuture>();
        configurator.AddFuture<ShakeFuture>();
        configurator.AddFuture<FryShakeFuture>();
    }
}
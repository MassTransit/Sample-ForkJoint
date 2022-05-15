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
public class ComboFuture_Specs :
    FutureTestFixture
{
    [Test]
    public async Task Should_complete()
    {
        var orderId = NewId.NextGuid();
        var orderLineId = NewId.NextGuid();

        var startedAt = DateTime.UtcNow;

        IRequestClient<OrderCombo> client = TestHarness.GetRequestClient<OrderCombo>();

        Response<ComboCompleted> response = await client.GetResponse<ComboCompleted>(new
        {
            OrderId = orderId,
            OrderLineId = orderLineId,
            Number = 5
        }, timeout: RequestTimeout.After(s: 5));

        Assert.That(response.Message.OrderId, Is.EqualTo(orderId));
        Assert.That(response.Message.OrderLineId, Is.EqualTo(orderLineId));
        Assert.That(response.Message.Created, Is.GreaterThan(startedAt));
        Assert.That(response.Message.Completed, Is.GreaterThan(response.Message.Created));
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
        configurator.AddFuture<ComboFuture>();
    }
}
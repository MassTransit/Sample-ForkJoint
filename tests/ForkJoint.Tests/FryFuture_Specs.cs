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
public class FryFuture_Specs :
    FutureTestFixture
{
    [Test]
    public async Task Should_complete()
    {
        var orderId = NewId.NextGuid();
        var orderLineId = NewId.NextGuid();

        var startedAt = DateTime.UtcNow;

        IRequestClient<OrderFry> client = TestHarness.GetRequestClient<OrderFry>();

        Response<FryCompleted> response = await client.GetResponse<FryCompleted>(new
        {
            OrderId = orderId,
            OrderLineId = orderLineId,
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
        collection.AddSingleton<IFryer, Fryer>();
    }

    protected override void ConfigureMassTransit(IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<CookFryConsumer>();
        configurator.AddFuture<FryFuture>();
    }
}
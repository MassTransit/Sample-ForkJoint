namespace ForkJoint.Api;

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;


public static class OrderRoutes
{
    public static IEndpointRouteBuilder MapOrderRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/order", SubmitOrder);

        return builder;
    }

    static async Task<IResult> SubmitOrder(Order order, IRequestClient<SubmitOrder> client, CancellationToken cancellationToken)
    {
        try
        {
            Response response = await client.GetResponse<OrderCompleted, OrderFaulted>(new
            {
                order.OrderId,
                order.Burgers,
                order.Fries,
                order.Shakes,
                order.FryShakes
            });

            return response switch
            {
                (_, OrderCompleted completed) => TypedResults.Ok(new
                {
                    completed.OrderId,
                    completed.Created,
                    completed.Completed,
                    LinesCompleted = completed.LinesCompleted.ToDictionary(x => x.Key, x => new
                    {
                        x.Value.Created,
                        x.Value.Completed,
                        x.Value.Description,
                    })
                }),
                (_, OrderFaulted faulted) => TypedResults.BadRequest(new
                {
                    faulted.OrderId,
                    faulted.Created,
                    faulted.Faulted,
                    LinesCompleted = faulted.LinesCompleted.ToDictionary(x => x.Key, x => new
                    {
                        x.Value.Created,
                        x.Value.Completed,
                        x.Value.Description,
                    }),
                    LinesFaulted = faulted.LinesFaulted.ToDictionary(x => x.Key, x => new
                    {
                        Faulted = x.Value.Timestamp,
                        Reason = x.Value.GetExceptionMessages(),
                    })
                }),
                _ => TypedResults.BadRequest()
            };
        }
        catch (RequestTimeoutException)
        {
            return TypedResults.Accepted($"/order/{order.OrderId}", new
            {
                order.OrderId,
                Accepted = order.Burgers.Select(x => x.BurgerId).ToArray()
            });
        }
    }


    public class Order
    {
        [Required]
        public Guid OrderId { get; init; }

        public Burger[] Burgers { get; init; }
        public Fry[] Fries { get; init; }
        public Shake[] Shakes { get; init; }
        public FryShake[] FryShakes { get; init; }
    }
}
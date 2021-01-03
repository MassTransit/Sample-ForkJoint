namespace ForkJoint.Api.Components.Consumers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using ItineraryPlanners;
    using MassTransit;
    using MassTransit.Courier;


    public class SubmitOrderConsumer :
        RoutingSlipRequestConsumer<SubmitOrder>
    {
        readonly IItineraryPlanner<Burger> _planner;

        public SubmitOrderConsumer(IItineraryPlanner<Burger> planner, IEndpointNameFormatter formatter)
            : base(formatter.Consumer<SubmitOrderResponseConsumer>())
        {
            _planner = planner;
        }

        protected override Task BuildItinerary(RoutingSlipBuilder builder, ConsumeContext<SubmitOrder> context)
        {
            builder.AddVariable("OrderId", context.Message.OrderId);

            if (context.ExpirationTime.HasValue)
                builder.AddVariable("Deadline", context.ExpirationTime.Value);

            var burger = context.Message.Burgers.FirstOrDefault();

            if (burger != null)
                _planner.ProduceItinerary(burger, builder);

            return Task.CompletedTask;
        }
    }
}
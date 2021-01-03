namespace ForkJoint.Api.Components.ItineraryPlanners
{
    using MassTransit.Courier;


    public interface IItineraryPlanner<in T>
    {
        void ProduceItinerary(T value, ItineraryBuilder builder);
    }
}
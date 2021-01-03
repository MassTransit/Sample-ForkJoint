namespace ForkJoint.Api.Components.ItineraryPlanners
{
    using MassTransit.Courier;


    public interface IItineraryPlanner<in T>
    {
        void PlanItinerary(T value, ItineraryBuilder builder);
    }
}
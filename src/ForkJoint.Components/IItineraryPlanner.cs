namespace ForkJoint.Components
{
    using System.Threading.Tasks;
    using MassTransit.Courier;


    public interface IItineraryPlanner<in T>
    {
        Task PlanItinerary(T value, ItineraryBuilder builder);
    }
}
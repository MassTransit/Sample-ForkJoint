namespace ForkJoint.Api.Components.Activities.GrillBurger
{
    public interface GrillBurgerLog
    {
        decimal Weight { get; }
        decimal Temperature { get; }
        bool Cheese { get; }
    }
}
namespace ForkJoint.Contracts
{
    public interface ShakeCompleted :
        OrderLineCompleted
    {
        string Flavor { get; }
        Size Size { get; }
    }
}
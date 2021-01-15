namespace ForkJoint.Contracts
{
    public interface FryCompleted :
        OrderLineCompleted
    {
        Size Size { get; }
    }
}
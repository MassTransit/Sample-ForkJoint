namespace ForkJoint.Contracts
{
    public interface OnionRingsCompleted :
        OrderLineCompleted
    {
        int Quantity { get; }
    }
}
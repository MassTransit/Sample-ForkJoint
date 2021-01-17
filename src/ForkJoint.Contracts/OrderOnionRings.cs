namespace ForkJoint.Contracts
{
    public interface OrderOnionRings :
        OrderLine
    {
        int Quantity { get; }
    }
}
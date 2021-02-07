namespace ForkJoint.Contracts
{
    public interface FryShakeCompleted :
        OrderLineCompleted
    {
        string Flavor { get; }
        Size Size { get; }
    }


    public interface ComboCompleted :
        OrderLineCompleted
    {
    }
}
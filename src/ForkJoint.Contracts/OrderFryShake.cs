namespace ForkJoint.Contracts
{
    public interface OrderFryShake :
        OrderLine
    {
        string Flavor { get; }
        Size Size { get; }
    }


    public interface OrderCombo :
        OrderLine
    {
        int Number { get; }
    }
}
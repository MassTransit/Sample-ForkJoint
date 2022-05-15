namespace ForkJoint.Contracts;

public interface OrderFry :
    OrderLine
{
    Size Size { get; }
}
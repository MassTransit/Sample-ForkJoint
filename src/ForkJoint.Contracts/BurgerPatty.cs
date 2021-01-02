namespace ForkJoint.Contracts
{
    public record BurgerPatty
    {
        public decimal Weight { get; init; }
        public decimal Temperature { get; init; }
        public bool Cheese { get; init; }
    }
}
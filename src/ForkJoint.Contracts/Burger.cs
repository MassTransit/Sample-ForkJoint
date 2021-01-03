namespace ForkJoint.Contracts
{
    public record Burger
    {
        public decimal Weight { get; init; } = 0.5m;
        public bool Lettuce { get; init; }
        public bool Cheese { get; init; }
        public bool Pickle { get; init; } = true;
        public bool Onion { get; init; } = true;
        public bool Ketchup { get; init; }
        public bool Mustard { get; init; } = true;
    }
}
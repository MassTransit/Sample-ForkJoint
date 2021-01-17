namespace ForkJoint.Components
{
    using System;
    using MassTransit.Internals.Extensions;
    using MassTransit.Registration;


    public readonly struct FutureLocation
    {
        public readonly Uri Address;
        public readonly Guid Id;

        public FutureLocation(Uri location)
        {
            if (!location.TryGetValueFromQueryString("id", out var value))
                throw new FormatException($"Location format invalid: {location}");

            Id = new Guid(value);
            Address = new Uri(location.GetLeftPart(UriPartial.Path));
        }

        public FutureLocation(Guid id, Uri address)
        {
            Id = id;
            Address = new Uri($"queue:{address.GetLastPart()}");
        }

        public static implicit operator Uri(FutureLocation location)
        {
            return new UriBuilder(location.Address) {Query = $"id={location.Id:N}"}.Uri;
        }
    }
}
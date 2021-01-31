namespace ForkJoint.Components
{
    using System;
    using MassTransit;
    using MassTransit.Internals.Extensions;
    using MassTransit.NewIdFormatters;
    using MassTransit.NewIdParsers;
    using MassTransit.Registration;


    public readonly struct FutureLocation
    {
        public readonly Uri Address;
        public readonly Guid Id;

        public FutureLocation(Uri location)
        {
            if (!location.TryGetValueFromQueryString("id", out var value))
                throw new FormatException($"Location format invalid: {location}");

            var parsedId = IdParser.Parse(value);
            Id = parsedId.ToGuid();

            Address = new Uri(location.GetLeftPart(UriPartial.Path));
        }

        public FutureLocation(Guid id, Uri address)
        {
            Id = id;
            Address = new Uri($"queue:{address.GetLastPart()}");
        }

        public static implicit operator Uri(FutureLocation location)
        {
            var newId = location.Id.ToNewId();
            var id = newId.ToString(IdFormatter);

            return new UriBuilder(location.Address) {Query = $"id={id}"}.Uri;
        }

        static readonly INewIdFormatter IdFormatter = new ZBase32Formatter();
        static readonly INewIdParser IdParser = new ZBase32Parser(true);
    }
}
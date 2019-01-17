namespace PostalRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;
    using NodaTime;

    public class BPostTimestamp : InstantValueObject<BPostTimestamp>
    {
        public BPostTimestamp([JsonProperty("value")] Instant date) : base(date) { }
    }
}

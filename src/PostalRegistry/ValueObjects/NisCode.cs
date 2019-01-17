namespace PostalRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class NisCode : StringValueObject<NisCode>
    {
        public NisCode([JsonProperty("value")] string nisCode) : base(nisCode) { }
    }
}

namespace PostalRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class Province : StringValueObject<Province>
    {
        public Province([JsonProperty("value")] string province) : base(province) { }
    }
}

namespace PostalRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class PostalCode : StringValueObject<PostalCode>
    {
        public PostalCode([JsonProperty("value")] string postalCode) : base(postalCode) { }
    }
}

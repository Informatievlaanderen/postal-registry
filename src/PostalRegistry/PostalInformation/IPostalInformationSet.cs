namespace PostalRegistry.PostalInformation
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public interface IPostalInformationSet : IAsyncRepository<PostalInformation, PostalCode> { }
}

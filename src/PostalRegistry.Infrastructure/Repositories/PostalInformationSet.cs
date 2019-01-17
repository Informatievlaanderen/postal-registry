namespace PostalRegistry.Infrastructure.Repositories
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using PostalInformation;
    using SqlStreamStore;

    public class PostalInformationSet : Repository<PostalInformation, PostalCode>, IPostalInformationSet
    {
        public PostalInformationSet(ConcurrentUnitOfWork unitOfWork, IStreamStore eventStore, EventMapping eventMapping, EventDeserializer eventDeserializer)
            : base(PostalInformation.Factory, unitOfWork, eventStore, eventMapping, eventDeserializer) { }
    }
}

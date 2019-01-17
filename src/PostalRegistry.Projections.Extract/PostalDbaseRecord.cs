namespace PostalRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class PostalDbaseRecord : DbaseRecord
    {
        public static readonly PostalDbaseSchema Schema = new PostalDbaseSchema();

        public DbaseString id { get; }
        public DbaseString postinfoid { get; }
        public DbaseDateTime versie { get; }
        public DbaseString postnaam { get; }
        public DbaseString status { get; }

        public PostalDbaseRecord()
        {
            id = new DbaseString(Schema.id);
            postinfoid = new DbaseString(Schema.postinfoid);
            versie = new DbaseDateTime(Schema.versie);
            postnaam = new DbaseString(Schema.postnaam);
            status = new DbaseString(Schema.status);

            Values = new DbaseFieldValue[]
            {
                id,
                postinfoid,
                versie,
                postnaam,
                status
            };
        }
    }
}

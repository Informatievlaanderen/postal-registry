namespace PostalRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class PostalDbaseRecord : DbaseRecord
    {
        public static readonly PostalDbaseSchema Schema = new PostalDbaseSchema();

        public DbaseString id { get; }
        public DbaseString postinfoid { get; }
        public DbaseDateTime versieid { get; }
        public DbaseString postnaam { get; }
        public DbaseString status { get; }

        public PostalDbaseRecord()
        {
            id = new DbaseString(Schema.id);
            postinfoid = new DbaseString(Schema.postinfoid);
            versieid = new DbaseDateTime(Schema.versieid);
            postnaam = new DbaseString(Schema.postnaam);
            status = new DbaseString(Schema.status);

            Values = new DbaseFieldValue[]
            {
                id,
                postinfoid,
                versieid,
                postnaam,
                status
            };
        }
    }
}

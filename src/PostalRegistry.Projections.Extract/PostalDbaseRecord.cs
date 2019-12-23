namespace PostalRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class PostalDbaseRecord : DbaseRecord
    {
        public static readonly PostalDbaseSchema Schema = new PostalDbaseSchema();

        public DbaseCharacter id { get; }
        public DbaseCharacter postinfoid { get; }
        public DbaseCharacter versieid { get; }
        public DbaseCharacter postnaam { get; }
        public DbaseCharacter status { get; }

        public PostalDbaseRecord()
        {
            id = new DbaseCharacter(Schema.id);
            postinfoid = new DbaseCharacter(Schema.postinfoid);
            versieid = new DbaseCharacter(Schema.versieid);
            postnaam = new DbaseCharacter(Schema.postnaam);
            status = new DbaseCharacter(Schema.status);

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

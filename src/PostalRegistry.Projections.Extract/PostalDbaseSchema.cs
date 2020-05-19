namespace PostalRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class PostalDbaseSchema : DbaseSchema
    {
        public DbaseField id => Fields[0];
        public DbaseField postinfoid => Fields[1];
        public DbaseField versieid => Fields[2];
        public DbaseField postnaam => Fields[3];
        public DbaseField status => Fields[4];

        public PostalDbaseSchema() => Fields = new[]
        {
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(id)), new DbaseFieldLength(254)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(postinfoid)), new DbaseFieldLength(4)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(versieid)), new DbaseFieldLength(25)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(postnaam)), new DbaseFieldLength(254)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(status)), new DbaseFieldLength(50))
        };
    }
}

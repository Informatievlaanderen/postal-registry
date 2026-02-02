namespace PostalRegistry.Projections.Feed.Contract
{
    public static class PostalEventTypes
    {
        public const string CreateV1 = "basisregisters.postalinformation.create.v1";
        public const string UpdateV1 = "basisregisters.postalinformation.update.v1";
        public const string DeleteV1 = "basisregisters.postalinformation.delete.v1";
        public const string TransformV1 = "basisregisters.postalinformation.transform.v1";
    }

    public static class PostalAttributeNames
    {
        public const string MunicipalityId = "gemeente.id";
        public const string PostalCode = "postcode";
        public const string StatusName = "postInfoStatus";
        public const string PostalNames = "postnamen";
    }
}

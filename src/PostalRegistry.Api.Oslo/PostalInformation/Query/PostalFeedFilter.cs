namespace PostalRegistry.Api.Oslo.PostalInformation.Query
{
    public sealed class PostalFeedFilter
    {
        public int? Page { get; set; }
        public long? FeedPosition { get; set; }
    }

    public class PostalPositionFilter
    {
        public long? Download { get; set; }
        public long? Sync { get; set; }
        public long? ChangeFeedId { get; set; }

        public bool HasMoreThanOneFilter =>
            (Download.HasValue ? 1 : 0)
            + (Sync.HasValue ? 1 : 0)
            + (ChangeFeedId.HasValue ? 1 : 0) > 1;
    }
}

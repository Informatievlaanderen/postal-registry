namespace PostalRegistry.Api.Oslo.Infrastructure.Options
{
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;

    public class ResponseOptions
    {
        public string Naamruimte { get; set; }
        public string VolgendeUrl { get; set; }
        public string DetailUrl { get; set; }
        public string ContextUrlList { get; set; }
        public string ContextUrlDetail { get; set; }

        public string GemeenteNaamruimte { get; set; }
        public string GemeenteDetailUrl { get; set; }

        public string PostInfoDetailAddressesLink { get; set; }

        public ChangeFeedConfig PostalFeed { get; set; }
    }
}

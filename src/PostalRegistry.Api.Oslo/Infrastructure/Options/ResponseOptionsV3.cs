namespace PostalRegistry.Api.Oslo.Infrastructure.Options
{
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;

    /// <summary>
    /// V3-specific response options. Bound to the "V3" configuration section so the
    /// V3 endpoints can advertise their own (v3) URLs independently from V2.
    /// </summary>
    public class ResponseOptionsV3
    {
        public string VolgendeUrl { get; set; }
        public string DetailUrl { get; set; }
        public string ContextUrlList { get; set; }
        public string ContextUrlDetail { get; set; }

        public string GemeenteDetailUrl { get; set; }

        public string PostInfoDetailAddressesLink { get; set; }

        public ChangeFeedConfig PostalFeed { get; set; }
    }
}

namespace PostalRegistry.Api.Legacy.Convertors
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;

    public static class PostalInformationStatusExtensions
    {
        public static PostInfoStatus ConvertFromPostalInformationStatus(this PostalInformationStatus postalInformationStatus)
        {
            switch (postalInformationStatus)
            {
                case PostalInformationStatus.Retired:
                    return PostInfoStatus.Gehistoreerd;

                default:
                case PostalInformationStatus.Current:
                    return PostInfoStatus.Gerealiseerd;
            }
        }
    }
}

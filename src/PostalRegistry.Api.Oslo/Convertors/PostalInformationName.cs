namespace PostalRegistry.Api.Oslo.Convertors
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using Projections.Legacy.PostalInformation;

    public static class PostalInformationNameExtensions
    {
        public static Postnaam ConvertFromPostalName(this PostalInformationName name)
            => new Postnaam(new GeografischeNaam(name.Name, name.Language.ConvertFromLanguage()));
    }
}

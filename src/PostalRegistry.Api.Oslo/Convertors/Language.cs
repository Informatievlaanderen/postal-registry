namespace PostalRegistry.Api.Oslo.Convertors
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;

    public static class LanguageExtensions
    {
        public static Taal ConvertFromLanguage(this Language language)
        {
            switch (language)
            {
                default:
                case Language.Dutch:
                    return Taal.NL;

                case Language.French:
                    return Taal.FR;

                case Language.German:
                    return Taal.DE;

                case Language.English:
                    return Taal.EN;
            }
        }

        public static Be.Vlaanderen.Basisregisters.GrAr.Oslo.Taal ConvertOsloFromLanguage(this Language language)
        {
            switch (language)
            {
                default:
                case Language.Dutch:
                    return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Taal.Nl;

                case Language.French:
                    return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Taal.Fr;

                case Language.German:
                    return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Taal.De;

                case Language.English:
                    return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Taal.En;
            }
        }
    }
}

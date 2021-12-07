namespace PostalRegistry.Api.Oslo.PostalInformation.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Swashbuckle.AspNetCore.Filters;

    public class TotalCountOsloResponseExample : IExamplesProvider<TotaalAantalResponse>
    {
        public TotaalAantalResponse GetExamples()
        {
            return new TotaalAantalResponse
            {
                Aantal = 574512
            };
        }
    }
}

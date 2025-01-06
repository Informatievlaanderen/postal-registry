namespace PostalRegistry.Api.Import.UpdatePostalNames
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;

    public sealed class UpdatePostalNamesRequest
    {
        public string? PostalCode { get; set; }

        public List<Postnaam> PostalNamesToAdd { get; set; } = new List<Postnaam>();
        public List<Postnaam> PostalNamesToRemove { get; set; } = new List<Postnaam>();

        public string? Reason { get; set; }
    }
}

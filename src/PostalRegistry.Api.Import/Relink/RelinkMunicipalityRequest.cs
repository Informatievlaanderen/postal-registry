namespace PostalRegistry.Api.Import.Relink
{
    public sealed class RelinkMunicipalityRequest
    {
        public string? PostalCode { get; set; }
        public string? NewNisCode { get; set; }

        public string? Reason { get; set; }
    }
}

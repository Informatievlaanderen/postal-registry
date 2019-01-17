namespace PostalRegistry.PostalInformation.Commands
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public interface IHasBPostProvenance
    {
        BPostTimestamp Timestamp { get; }

        Modification Modification { get; }
    }
}

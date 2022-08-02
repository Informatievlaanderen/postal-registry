namespace PostalRegistry.PostalInformation
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands;

    public class BPostPostalInformationProvenanceFactory : IProvenanceFactory<PostalInformation>
    {
        public bool CanCreateFrom<TCommand>() => typeof(IHasBPostProvenance).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(object provenanceHolder, PostalInformation aggregate)
        {
            if (!(provenanceHolder is IHasBPostProvenance bpostProvenance))
            {
                throw new InvalidOperationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");
            }

            return new Provenance(
                bpostProvenance.Timestamp,
                Application.BPost,
                Reason.CentralManagementBPost,
                new Operator("Postal information loading service"),
                bpostProvenance.Modification,
                Organisation.DePost);
        }
    }
}

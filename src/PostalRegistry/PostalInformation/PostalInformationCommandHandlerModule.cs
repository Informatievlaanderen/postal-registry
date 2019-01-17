namespace PostalRegistry.PostalInformation
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands.BPost;
    using Commands.Crab;

    public sealed class PostalInformationCommandHandlerModule : ProvenanceCommandHandlerModule<PostalInformation>
    {
        public PostalInformationCommandHandlerModule(
            Func<IPostalInformationSet> getPostalInformationSet,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            ReturnHandler<CommandMessage> finalHandler = null) :
            base(
                getUnitOfWork,
                finalHandler,
                new BPostPostalInformationProvenanceFactory(),
                new CrabPostalInformationProvenanceFactory())
        {
            For<ImportPostalInformationFromBPost>()
                .Handle(async (message, ct) =>
                {
                    var postalCode = message.Command.PostalCode;
                    var postalInformationSet = getPostalInformationSet();
                    var postalInformation = await postalInformationSet.GetOptionalAsync(postalCode, ct);

                    if (!postalInformation.HasValue)
                    {
                        postalInformation = new Optional<PostalInformation>(PostalInformation.Register(postalCode));
                        postalInformationSet.Add(postalCode, postalInformation.Value);
                    }

                    postalInformation.Value.ImportPostalInformationFromBPost(
                        message.Command.PostalCode,
                        message.Command.PostalNames,
                        message.Command.IsSubMunicipality,
                        message.Command.Province,
                        message.Command.Modification);
                });

            For<ImportPostalInformationFromCrab>()
                .Handle(async (mesage, ct) =>
                {
                    // need to use the subcanton => in crab postcode = 1030, subcanton = 1031
                    // in bpost postcode = 1031
                    var postalCode = new PostalCode(mesage.Command.SubCantonCode);
                    var postalInformation = await getPostalInformationSet().GetOptionalAsync(postalCode, ct);

                    if (!postalInformation.HasValue) // Crab has possible outdated postalcodes
                        return;

                    postalInformation.Value.ImportPostalInformationFromCrab(
                        mesage.Command.PostalCode,
                        mesage.Command.SubCantonId,
                        mesage.Command.SubCantonCode,
                        mesage.Command.NisCode,
                        mesage.Command.MunicipalityName,
                        mesage.Command.Lifetime,
                        mesage.Command.Timestamp,
                        mesage.Command.Operator,
                        mesage.Command.Modification,
                        mesage.Command.Organisation);
                });
        }
    }
}

namespace PostalRegistry.PostalInformation
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands;
    using Commands.BPost;
    using Commands.Crab;
    using SqlStreamStore;

    public sealed class PostalInformationCommandHandlerModule : CommandHandlerModule
    {
        public PostalInformationCommandHandlerModule(
            Func<IPostalInformationSet> getPostalInformationSet,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            BPostPostalInformationProvenanceFactory bpostProvenanceFactory,
            CrabPostalInformationProvenanceFactory crabProvenanceFactory,
            PostalInformationProvenanceFactory postalInformationProvenanceFactory)
        {
            For<ImportPostalInformationFromBPost>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, bpostProvenanceFactory)
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
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, crabProvenanceFactory)
                .Handle(async (message, ct) =>
                {
                    // need to use the subcanton => in crab postcode = 1030, subcanton = 1031
                    // in bpost postcode = 1031
                    var postalCode = new PostalCode(message.Command.SubCantonCode);
                    var postalInformation = await getPostalInformationSet().GetOptionalAsync(postalCode, ct);

                    if (!postalInformation.HasValue) // Crab has possible outdated postalcodes
                        return;

                    postalInformation.Value.ImportPostalInformationFromCrab(
                        message.Command.PostalCode,
                        message.Command.SubCantonId,
                        message.Command.SubCantonCode,
                        message.Command.NisCode,
                        message.Command.MunicipalityName,
                        message.Command.Lifetime,
                        message.Command.Timestamp,
                        message.Command.Operator,
                        message.Command.Modification,
                        message.Command.Organisation);
                });

            For<RelinkMunicipality>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, postalInformationProvenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var postalCode = new PostalCode(message.Command.PostalCode);
                    var postalInformation = await getPostalInformationSet().GetAsync(postalCode, ct);

                    postalInformation.RelinkMunicipality(message.Command.NewNisCode);
                });

            For<UpdatePostalNames>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, postalInformationProvenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var postalCode = new PostalCode(message.Command.PostalCode);
                    var postalInformation = await getPostalInformationSet().GetAsync(postalCode, ct);

                    postalInformation.UpdatePostalNames(message.Command.PostalNamesToAdd, message.Command.PostalNamesToRemove);
                });

            For<DeletePostalInformation>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, postalInformationProvenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var postalCode = new PostalCode(message.Command.PostalCode);
                    var postalInformation = await getPostalInformationSet().GetAsync(postalCode, ct);

                    postalInformation.Delete();
                });
        }
    }
}

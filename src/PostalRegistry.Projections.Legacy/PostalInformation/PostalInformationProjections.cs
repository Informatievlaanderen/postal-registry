namespace PostalRegistry.Projections.Legacy.PostalInformation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;
    using PostalRegistry.PostalInformation.Events;
    using PostalRegistry.PostalInformation.Events.BPost;
    using PostalRegistry.PostalInformation.Events.Crab;

    [ConnectedProjectionName("API endpoint detail/lijst postinfo")]
    [ConnectedProjectionDescription("Projectie die de postinfo data voor het postinfo detail & lijst voorziet.")]
    public class PostalInformationProjections : ConnectedProjection<LegacyContext>
    {
        public PostalInformationProjections()
        {
            When<Envelope<PostalInformationWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .PostalInformation
                    .AddAsync(
                        new PostalInformation
                        {
                            PostalCode = message.Message.PostalCode
                        }, ct);
            });

            When<Envelope<MunicipalityWasAttached>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformation(
                    message.Message.PostalCode,
                    postalInformation =>
                    {
                        postalInformation.NisCode = message.Message.NisCode;
                        UpdateVersionTimestamp(postalInformation, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<MunicipalityWasRelinked>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformation(
                    message.Message.PostalCode,
                    postalInformation =>
                    {
                        postalInformation.NisCode = message.Message.NewNisCode;
                        UpdateVersionTimestamp(postalInformation, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<PostalInformationWasRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformation(
                    message.Message.PostalCode,
                    postalInformation =>
                    {
                        postalInformation.IsRetired = false;
                        UpdateVersionTimestamp(postalInformation, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<PostalInformationPostalNameWasAdded>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformation(
                    message.Message.PostalCode,
                    postalInformation =>
                    {
                        if (postalInformation.PostalNames != null
                            && postalInformation.PostalNames.Any(p => p.Name.Equals(message.Message.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            return;
                        }

                        var postalInformationName = new PostalInformationName(
                            message.Message.Name,
                            message.Message.Name.RemoveDiacritics(),
                            postalInformation.PostalCode,
                            message.Message.Language);

                        if (postalInformation.PostalNames == null)
                        {
                            postalInformation.PostalNames = new List<PostalInformationName> { postalInformationName };
                        }
                        else
                        {
                            postalInformation.PostalNames.Add(postalInformationName);
                        }

                        UpdateVersionTimestamp(postalInformation, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<PostalInformationPostalNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformation(
                    message.Message.PostalCode,
                    postalInformation =>
                    {
                        postalInformation.PostalNames.RemoveAll(r => r.Name == message.Message.Name);
                        UpdateVersionTimestamp(postalInformation, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<PostalInformationWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformation(
                    message.Message.PostalCode,
                    postalInformation =>
                    {
                        postalInformation.IsRetired = true;
                        UpdateVersionTimestamp(postalInformation, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<PostalInformationWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformation(
                    message.Message.PostalCode,
                    postalInformation =>
                    {
                        postalInformation.IsRemoved = true;
                        UpdateVersionTimestamp(postalInformation, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<PostalInformationWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<PostalInformationWasImportedFromBPost>>(async (context, message, ct) => await DoNothing());
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }

        private static void UpdateVersionTimestamp(PostalInformation postalInformation, Instant versionTimestamp)
            => postalInformation.VersionTimestamp = versionTimestamp;
    }
}

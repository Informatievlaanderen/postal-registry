namespace PostalRegistry.Projections.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Infrastructure;
    using Microsoft.Extensions.Options;
    using NodaTime;
    using PostalInformation.Events;

    [ConnectedProjectionName("Integratie postinfo latest item")]
    [ConnectedProjectionDescription("Projectie die de laatste postinfo data voor de integratie database bijhoudt.")]
    public sealed class PostalLatestItemProjections : ConnectedProjection<IntegrationContext>
    {
        public PostalLatestItemProjections(IOptions<IntegrationOptions> options)
        {
            When<Envelope<PostalInformationWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .PostalLatestItems
                    .AddAsync(
                        new PostalLatestItem
                        {
                            PostalCode = message.Message.PostalCode,
                            Namespace = options.Value.Namespace,
                            PuriId = $"{options.Value.Namespace}/{message.Message.PostalCode}",
                            VersionTimestamp = message.Message.Provenance.Timestamp
                        }, ct);
            });

            When<Envelope<MunicipalityWasAttached>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostal(
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
                await context.FindAndUpdatePostal(
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
                await context.FindAndUpdatePostal(
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
                await context.FindAndUpdatePostal(
                    message.Message.PostalCode,
                    postalInformation =>
                    {
                        if (postalInformation.PostalNames != null
                            && postalInformation.PostalNames.Any(p =>
                                p.Name.Equals(message.Message.Name, StringComparison.OrdinalIgnoreCase)))
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
                await context.FindAndUpdatePostal(
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
                await context.FindAndUpdatePostal(
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
                await context.DeletePostal(message.Message.PostalCode, ct);
            });
        }

        private static void UpdateVersionTimestamp(PostalLatestItem postal, Instant versionTimestamp)
            => postal.VersionTimestamp = versionTimestamp;
    }
}

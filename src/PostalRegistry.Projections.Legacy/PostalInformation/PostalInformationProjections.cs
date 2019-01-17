namespace PostalRegistry.Projections.Legacy.PostalInformation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;
    using PostalRegistry.PostalInformation.Events;

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

            When<Envelope<MunicipalityWasLinkedToPostalInformation>>(async (context, message, ct) =>
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

            When<Envelope<PostalInformationBecameCurrent>>(async (context, message, ct) =>
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
                        if (postalInformation.PostalNames != null &&
                            postalInformation.PostalNames.Any(p => p.Name.Equals(message.Message.Name, StringComparison.OrdinalIgnoreCase)))
                            return;

                        var postalInformationName = new PostalInformationName(message.Message.Name, postalInformation.PostalCode, message.Message.Language);

                        if (postalInformation.PostalNames == null)
                            postalInformation.PostalNames = new List<PostalInformationName> { postalInformationName };
                        else
                            postalInformation.PostalNames.Add(postalInformationName);

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
        }

        private static void UpdateVersionTimestamp(PostalInformation postalInformation, Instant versionTimestamp)
            => postalInformation.VersionTimestamp = versionTimestamp;
    }
}

namespace PostalRegistry.Projections.Extract.PostalInformationExtract
{
    using System;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Options;
    using NodaTime;
    using PostalInformation.Events;
    using PostalInformation.Events.BPost;
    using PostalInformation.Events.Crab;

    public class PostalInformationExtractProjections : ConnectedProjection<ExtractContext>
    {
        // TODO: Probably need to get these from enums from config
        private const string Realized = "Gerealiseerd";
        private const string Retired = "Gehistoreerd";

        private readonly Encoding _encoding;

        public PostalInformationExtractProjections(IOptions<ExtractConfig> extractConfig, Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<PostalInformationWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .PostalInformationExtract
                    .AddAsync(new PostalInformationExtractItem
                    {
                        PostalCode = message.Message.PostalCode,
                        DbaseRecord = new PostalDbaseRecord
                        {
                            id = { Value = $"{extractConfig.Value.DataVlaanderenNamespace}/{message.Message.PostalCode}" },
                            postinfoid = { Value = message.Message.PostalCode },
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() },
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<PostalInformationBecameCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformationExtract(
                    message.Message.PostalCode,
                    postalInformationSet =>
                    {
                        UpdateStatus(postalInformationSet, Realized);
                        UpdateVersie(postalInformationSet, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<PostalInformationWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformationExtract(
                    message.Message.PostalCode,
                    postalInformationSet =>
                    {
                        UpdateStatus(postalInformationSet, Retired);
                        UpdateVersie(postalInformationSet, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<PostalInformationPostalNameWasAdded>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformationExtract(
                    message.Message.PostalCode,
                    postalInformation =>
                    {
                        UpdateRecord(postalInformation, postalInfo =>
                            {
                                // Grab the first postal information, if the name is empty, update that one, otherwise add a new one
                                if (string.IsNullOrWhiteSpace(postalInfo.postnaam.Value))
                                    postalInfo.postnaam.Value = message.Message.Name;
                                else
                                    postalInfo.postnaam.Value = postalInfo.postnaam.Value + "/" + message.Message.Name;
                            });

                        UpdateVersie(postalInformation, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<PostalInformationPostalNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformationExtract(
                    message.Message.PostalCode,
                    postalInformation =>
                    {
                        UpdateRecord(postalInformation, postalInfo =>
                        {
                            var index = postalInfo.postnaam.Value.IndexOf(message.Message.Name);
                            var count = message.Message.Name.Length;
                            if (postalInfo.postnaam.Value.Contains('/')) //Has multiple names
                            {
                                if (index == 0)
                                    count++;
                                else
                                    index--;
                            }

                            postalInfo.postnaam.Value = postalInfo.postnaam.Value.Remove(index, count);

                        });

                        UpdateVersie(postalInformation, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<MunicipalityWasAttached>>(async (context, message, ct) => DoNothing());
            When<Envelope<PostalInformationWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<PostalInformationWasImportedFromBPost>>(async (context, message, ct) => DoNothing());
        }

        private void UpdateStatus(PostalInformationExtractItem postalInformation, string status)
            => UpdateRecord(postalInformation, record => { record.status.Value = status; });

        private void UpdateVersie(PostalInformationExtractItem postalInformation, Instant timestamp)
            => UpdateRecord(postalInformation, record => record.versieid.SetValue(timestamp.ToBelgianDateTimeOffset()));

        private void UpdateRecord(PostalInformationExtractItem postalInformation, Action<PostalDbaseRecord> updateFunc)
        {
            var record = new PostalDbaseRecord();
            record.FromBytes(postalInformation.DbaseRecord, _encoding);

            updateFunc(record);

            postalInformation.DbaseRecord = record.ToBytes(_encoding);
        }

        private static void DoNothing() { }
    }
}

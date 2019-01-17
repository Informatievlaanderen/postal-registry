namespace PostalRegistry.Projections.Extract.PostalInformationExtract
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;
    using PostalInformation.Events;

    public class PostalInformationExtractProjections : ConnectedProjection<ExtractContext>
    {
        // TODO: Probably need to get these from enums and data vlaanderen from config
        private const string Realized = "Gerealiseerd";
        private const string Retired = "Gehistoreerd";
        private const string IdUri = "https://data.vlaanderen.be/id/postinfo";

        private readonly Encoding _encoding;

        public PostalInformationExtractProjections(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<PostalInformationWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .PostalInformationExtract
                    .AddAsync(new PostalInformationExtractItem
                    {
                        PostalCode = message.Message.PostalCode,
                        PostName = string.Empty,
                        DbaseRecord = new PostalDbaseRecord
                        {
                            id = { Value = $"{IdUri}/{message.Message.PostalCode}" },
                            postinfoid = { Value = message.Message.PostalCode },
                            versie = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().DateTime },
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
                    async postalInformationSet =>
                    {
                        var postalInformationDbaseRecord = new PostalDbaseRecord();
                        var firstPostalInformation = postalInformationSet.First();
                        postalInformationDbaseRecord.FromBytes(firstPostalInformation.DbaseRecord, _encoding);

                        // Grab the first postal information, if the name is empty, update that one, otherwise add a new one
                        if (string.IsNullOrWhiteSpace(postalInformationDbaseRecord.postnaam.Value))
                        {
                            postalInformationDbaseRecord.postnaam.Value = message.Message.Name;
                            firstPostalInformation.DbaseRecord = postalInformationDbaseRecord.ToBytes(_encoding);
                        }
                        else
                        {
                            await context
                                .PostalInformationExtract
                                .AddAsync(new PostalInformationExtractItem
                                {
                                    PostalCode = message.Message.PostalCode,
                                    PostName = message.Message.Name,
                                    DbaseRecord = new PostalDbaseRecord
                                    {
                                        versie = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().DateTime },
                                        id = { Value = $"{IdUri}/{message.Message.PostalCode}" },
                                        postinfoid = { Value = message.Message.PostalCode },
                                        postnaam = { Value = message.Message.Name },
                                        status = { Value = postalInformationDbaseRecord.status.Value }
                                    }.ToBytes(_encoding)
                                }, ct);
                        }
                    },
                    ct);
            });

            When<Envelope<PostalInformationPostalNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformationExtract(
                    message.Message.PostalCode,
                    postalInformationSet =>
                    {
                        if (postalInformationSet.Count == 1)
                        {
                            var postalInformationDbaseRecord = new PostalDbaseRecord();
                            var firstPostalInformation = postalInformationSet.First();

                            postalInformationDbaseRecord.FromBytes(firstPostalInformation.DbaseRecord, _encoding);
                            postalInformationDbaseRecord.postnaam.Value = string.Empty;
                            firstPostalInformation.DbaseRecord = postalInformationDbaseRecord.ToBytes(_encoding);
                        }
                        else
                        {
                            var postalInformationToRemove = postalInformationSet.First(x => x.PostName == message.Message.Name);
                            context.Remove(postalInformationToRemove);
                        }
                    },
                    ct);
            });
        }

        private void UpdateStatus(IEnumerable<PostalInformationExtractItem> postalInformationSet, string status)
        {
            foreach (var postalInformation in postalInformationSet)
                UpdateRecord(postalInformation, record => { record.status.Value = status; });
        }

        private void UpdateVersie(IEnumerable<PostalInformationExtractItem> postalInformationSet, Instant timestamp)
        {
            foreach (var postalInformation in postalInformationSet)
                UpdateRecord(postalInformation, record => record.versie.Value = timestamp.ToBelgianDateTimeOffset().DateTime);
        }

        private void UpdateRecord(PostalInformationExtractItem postalInformation, Action<PostalDbaseRecord> updateFunc)
        {
            var record = new PostalDbaseRecord();
            record.FromBytes(postalInformation.DbaseRecord, _encoding);

            updateFunc(record);

            postalInformation.DbaseRecord = record.ToBytes(_encoding);
        }
    }
}

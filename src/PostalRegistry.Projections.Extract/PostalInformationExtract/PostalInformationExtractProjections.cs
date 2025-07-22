namespace PostalRegistry.Projections.Extract.PostalInformationExtract
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Options;
    using NodaTime;
    using PostalInformation.Events;
    using PostalInformation.Events.BPost;
    using PostalInformation.Events.Crab;

    [ConnectedProjectionName("Extract postinfo")]
    [ConnectedProjectionDescription("Projectie die de postinfo data voor het postinfo extract voorziet.")]
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
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<PostalInformationWasRealized>>(async (context, message, ct) =>
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

            When<Envelope<PostalInformationWasRemoved>>(async (context, message, ct) =>
            {
                await context.DeletePostalInformationExtract(message.Message.PostalCode, ct);
            });

            When<Envelope<PostalInformationPostalNameWasAdded>>(async (context, message, ct) =>
            {
                await context.FindAndUpdatePostalInformationExtract(
                    message.Message.PostalCode,
                    postalInformation =>
                    {
                        UpdateRecord(postalInformation, postalInformationRecord =>
                        {
                            // Grab the first postal information, if the name is empty, update that one, otherwise add a new one
                            if (string.IsNullOrWhiteSpace(postalInformationRecord.postnaam.Value))
                            {
                                postalInformationRecord.postnaam.Value = message.Message.Name;
                            }
                            else
                            {
                                postalInformationRecord.postnaam.Value = postalInformationRecord.postnaam.Value + "/" + message.Message.Name;
                            }
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
                        UpdateRecord(postalInformation, postalInformationRecord =>
                        {
                            var index = postalInformationRecord.postnaam.Value.IndexOf(message.Message.Name, StringComparison.OrdinalIgnoreCase);
                            var count = message.Message.Name.Length;
                            if (postalInformationRecord.postnaam.Value.Contains('/')) //Has multiple names
                            {
                                if (index == 0)
                                {
                                    count++;
                                }
                                else
                                {
                                    index--;
                                }
                            }

                            postalInformationRecord.postnaam.Value = postalInformationRecord.postnaam.Value.Remove(index, count);
                        });

                        UpdateVersie(postalInformation, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<MunicipalityWasAttached>>(async (context, message, ct) => await DoNothing());
            When<Envelope<PostalInformationWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<PostalInformationWasImportedFromBPost>>(async (context, message, ct) => await DoNothing());
        }

        private void UpdateStatus(PostalInformationExtractItem postalInformation, string status)
            => UpdateRecord(postalInformation, record => { record.status.Value = status; });

        private void UpdateVersie(PostalInformationExtractItem postalInformation, Instant timestamp)
            => UpdateRecord(postalInformation, record => record.versieid.SetValue(timestamp.ToBelgianDateTimeOffset()));

        private void UpdateRecord(PostalInformationExtractItem postalInformation, Action<PostalDbaseRecord> updateFunc)
        {
            var record = new PostalDbaseRecord();
            record.FromBytes(postalInformation.DbaseRecord!, _encoding);

            updateFunc(record);

            postalInformation.DbaseRecord = record.ToBytes(_encoding);
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}

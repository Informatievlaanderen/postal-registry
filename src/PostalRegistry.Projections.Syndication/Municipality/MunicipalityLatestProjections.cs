namespace PostalRegistry.Projections.Syndication.Municipality
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;

    public class MunicipalityLatestProjections : AtomEntryProjectionHandlerModule<MunicipalityEvent, SyndicationContent<Gemeente>, SyndicationContext>
    {
        public MunicipalityLatestProjections()
        {
            When(MunicipalityEvent.MunicipalityWasRegistered, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNisCodeWasDefined, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNisCodeWasCorrected, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityWasNamed, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNameWasCleared, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNameWasCorrected, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNameWasCorrectedToCleared, AddSyndicationItemEntry);

            // only version is updated
            When(MunicipalityEvent.MunicipalityOfficialLanguageWasAdded, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityOfficialLanguageWasRemoved, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityFacilitiesLanguageWasAdded, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityFacilitiesLanguageWasRemoved, AddSyndicationItemEntry);
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<SyndicationContent<Gemeente>> entry, SyndicationContext context, CancellationToken ct)
        {
            var municipalityLatestItem = await context
                .MunicipalityLatestItems
                .FindAsync(entry.Content.Object.Id);

            if (municipalityLatestItem == null)
            {
                municipalityLatestItem = new MunicipalityLatestItem
                {
                    MunicipalityId = entry.Content.Object.Id,
                    NisCode = entry.Content.Object.Identificator?.ObjectId,
                    LastUpdatedOn = entry.FeedEntry.LastUpdated,
                    Version = entry.Content.Object.Identificator?.Versie,
                    Position = long.Parse(entry.FeedEntry.Id)
                };

                UpdateNamesByGemeentenamen(municipalityLatestItem, entry.Content.Object.Gemeentenamen);

                await context
                    .MunicipalityLatestItems
                    .AddAsync(municipalityLatestItem, ct);
            }
            else
            {
                municipalityLatestItem.NisCode = entry.Content.Object.Identificator?.ObjectId;
                municipalityLatestItem.LastUpdatedOn = entry.FeedEntry.LastUpdated;
                municipalityLatestItem.Version = entry.Content.Object.Identificator?.Versie;
                municipalityLatestItem.Position = long.Parse(entry.FeedEntry.Id);

                UpdateNamesByGemeentenamen(municipalityLatestItem, entry.Content.Object.Gemeentenamen);
            }
        }

        private static void UpdateNamesByGemeentenamen(MunicipalityLatestItem syndicationItem, List<GeografischeNaam> gemeentenamen)
        {
            if (gemeentenamen == null || !gemeentenamen.Any())
                return;

            foreach (var naam in gemeentenamen)
            {
                switch (naam.Taal)
                {
                    default:
                    case Taal.NL:
                        syndicationItem.NameDutch = naam.Spelling;
                        syndicationItem.NameDutchSearch = naam.Spelling.RemoveDiacritics();
                        break;

                    case Taal.FR:
                        syndicationItem.NameFrench = naam.Spelling;
                        syndicationItem.NameFrenchSearch = naam.Spelling.RemoveDiacritics();
                        break;

                    case Taal.DE:
                        syndicationItem.NameGerman = naam.Spelling;
                        syndicationItem.NameGermanSearch = naam.Spelling.RemoveDiacritics();
                        break;

                    case Taal.EN:
                        syndicationItem.NameEnglish = naam.Spelling;
                        syndicationItem.NameEnglishSearch = naam.Spelling.RemoveDiacritics();
                        break;
                }
            }
        }
    }
}

namespace PostalRegistry.Api.Legacy.PostalInformation.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Mime;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Convertors;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Query;
    using Swashbuckle.AspNetCore.Filters;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Syndication.Provenance;

    public static class PostalInformationSyndicationResponse
    {
        public static async Task WritePostalInfo(
            this ISyndicationFeedWriter writer,
            IOptions<ResponseOptions> responseOptions,
            AtomFormatter formatter,
            string category,
            PostalInformationSyndicationQueryResult postalInformation)
        {
            var item = new SyndicationItem
            {
                Id = postalInformation.Position.ToString(CultureInfo.InvariantCulture),
                Title = $"{postalInformation.ChangeType}-{postalInformation.Position}",
                Published = postalInformation.RecordCreatedAt.ToDateTimeOffset(),
                LastUpdated = postalInformation.LastChangedOn.ToDateTimeOffset(),
                Description = BuildDescription(postalInformation, responseOptions.Value.Naamruimte)
            };

            if (!string.IsNullOrWhiteSpace(postalInformation.PostalCode))
            {
                item.AddLink(
                    new SyndicationLink(
                        new Uri($"{responseOptions.Value.Naamruimte}/{postalInformation.PostalCode}"),
                        AtomLinkTypes.Related));

                item.AddLink(
                    new SyndicationLink(
                        new Uri(string.Format(responseOptions.Value.DetailUrl, postalInformation.PostalCode)),
                        AtomLinkTypes.Self));

                item.AddLink(
                    new SyndicationLink(
                        new Uri(string.Format($"{responseOptions.Value.DetailUrl}.xml", postalInformation.PostalCode)),
                        AtomLinkTypes.Alternate)
                    { MediaType = MediaTypeNames.Application.Xml });

                item.AddLink(
                    new SyndicationLink(
                        new Uri(string.Format($"{responseOptions.Value.DetailUrl}.json", postalInformation.PostalCode)),
                        AtomLinkTypes.Alternate)
                    { MediaType = MediaTypeNames.Application.Json });
            }

            item.AddCategory(
                new SyndicationCategory(category));

            item.AddContributor(
                new SyndicationPerson(
                    "agentschap Informatie Vlaanderen",
                    "informatie.vlaanderen@vlaanderen.be",
                    AtomContributorTypes.Author));

            await writer.Write(new SyndicationContent(formatter.CreateContent(item)));
        }

        private static string BuildDescription(PostalInformationSyndicationQueryResult postalInformation, string naamruimte)
        {
            var content = new PostalInfoSyndicationContent(
                naamruimte,
                postalInformation.PostalCode,
                postalInformation.LastChangedOn.ToBelgianDateTimeOffset(),
                postalInformation.Status,
                postalInformation.PostalNames,
                postalInformation.MunicipalityOsloId,
                postalInformation.Organisation,
                postalInformation.Plan);

            return content.ToXml();
        }
    }

    [DataContract(Name = "PostInfo", Namespace = "")]
    public class PostalInfoSyndicationContent : SyndicationContentBase
    {
        /// <summary>
        /// De technische id van de postinfo.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public string PostalCode { get; set; }

        /// <summary>
        /// De identificator van de postcode.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        public Identificator Identificator { get; set; }

        /// <summary>
        /// De namen van het gebied dat de postcode beslaat, in meerdere talen.
        /// </summary>
        [DataMember(Name = "Postnamen", Order = 2)]
        public List<Postnaam> PostalNames { get; set; }

        /// <summary>
        /// De huidige fase in de doorlooptijd van de postcode.
        /// </summary>
        [DataMember(Name = "PostInfoStatus", Order = 3)]
        public PostInfoStatus? Status { get; set; }

        /// <summary>
        /// De NisCode van de gemeente waarmee de postcode verwant is.
        /// </summary>
        [DataMember(Name = "NisCode", Order = 3)]
        public string MunicipalityOsloId { get; set; }

        /// <summary>
        /// Creatie data ivm het item.
        /// </summary>
        [DataMember(Name = "Creatie", Order = 4)]
        public Provenance Provenance { get; set; }

        public PostalInfoSyndicationContent(
            string naamruimte,
            string postcode,
            DateTimeOffset? version,
            PostalInformationStatus? status,
            IEnumerable<PostalName> postalNames,
            string municipalityOsloId,
            Organisation? organisation,
            Plan? plan)
        {
            PostalCode = postcode;
            Identificator = new Identificator(naamruimte, postcode, version);
            Status = status?.ConvertFromPostalInformationStatus();
            MunicipalityOsloId = municipalityOsloId;
            PostalNames = postalNames?
                              .Select(name => new Postnaam(new GeografischeNaam(name.Name, name.Language.ConvertFromLanguage())))
                              .ToList()
                          ?? new List<Postnaam>();
            Provenance = new Provenance(organisation, plan);
        }
    }

    public class PostalInformationSyndicationResponseExamples : IExamplesProvider
    {
        private readonly ResponseOptions _responseOptions;

        public PostalInformationSyndicationResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public object GetExamples() => new { };
    }
}

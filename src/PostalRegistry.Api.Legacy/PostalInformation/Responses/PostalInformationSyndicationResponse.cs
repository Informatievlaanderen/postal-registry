namespace PostalRegistry.Api.Legacy.PostalInformation.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Xml;
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
                Published = postalInformation.RecordCreatedAt.ToBelgianDateTimeOffset(),
                LastUpdated = postalInformation.LastChangedOn.ToBelgianDateTimeOffset(),
                Description = BuildDescription(postalInformation, responseOptions.Value.Naamruimte)
            };

            if (!string.IsNullOrWhiteSpace(postalInformation.PostalCode))
            {
                item.AddLink(
                    new SyndicationLink(
                        new Uri($"{responseOptions.Value.Naamruimte}/{postalInformation.PostalCode}"),
                        AtomLinkTypes.Related));

                //item.AddLink(
                //    new SyndicationLink(
                //        new Uri(string.Format(responseOptions.Value.DetailUrl, postalInformation.PostalCode)),
                //        AtomLinkTypes.Self));

                //item.AddLink(
                //    new SyndicationLink(
                //        new Uri(string.Format($"{responseOptions.Value.DetailUrl}.xml", postalInformation.PostalCode)),
                //        AtomLinkTypes.Alternate)
                //    { MediaType = MediaTypeNames.Application.Xml });

                //item.AddLink(
                //    new SyndicationLink(
                //        new Uri(string.Format($"{responseOptions.Value.DetailUrl}.json", postalInformation.PostalCode)),
                //        AtomLinkTypes.Alternate)
                //    { MediaType = MediaTypeNames.Application.Json });
            }

            item.AddCategory(
                new SyndicationCategory(category));

            /*item.AddContributor(
                new SyndicationPerson(
                    postalInformation.Organisation == null ? Organisation.Unknown.ToName() : postalInformation.Organisation.Value.ToName(),
                    string.Empty,
                    AtomContributorTypes.Author));*/

            await writer.Write(item);
        }

        private static string BuildDescription(PostalInformationSyndicationQueryResult postalInformation, string naamruimte)
        {
            if (!postalInformation.ContainsEvent && !postalInformation.ContainsObject)
                return "No data embedded";

            var content = new SyndicationContent();
            if (postalInformation.ContainsObject)
                content.Object = new PostalInfoSyndicationContent(
                    naamruimte,
                    postalInformation.PostalCode,
                    postalInformation.LastChangedOn.ToBelgianDateTimeOffset(),
                    postalInformation.Status,
                    postalInformation.PostalNames);

            /*if (postalInformation.ContainsEvent)
            {
                var doc = new XmlDocument();
                doc.LoadXml(postalInformation.EventDataAsXml);
                content.Event = doc.DocumentElement;
            }*/

            return content.ToXml();
        }
    }

    [DataContract(Name = "Content", Namespace = "")]
    public class SyndicationContent : SyndicationContentBase
    {
        [DataMember(Name = "Event")]
        public XmlElement Event { get; set; }

        [DataMember(Name = "Object")]
        public PostalInfoSyndicationContent Object { get; set; }
    }

    [DataContract(Name = "PostInfo", Namespace = "")]
    public class PostalInfoSyndicationContent
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
        public PostinfoIdentificator Identificator { get; set; }

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
        public string MunicipalityNisCode { get; set; }

        /// <summary>
        /// Creatie data ivm het item.
        /// </summary>
        [DataMember(Name = "Creatie", Order = 4)]
        public Provenance Provenance { get; set; }

        public PostalInfoSyndicationContent(
            string naamruimte,
            string postcode,
            DateTimeOffset version,
            PostalInformationStatus? status,
            IEnumerable<PostalName> postalNames)
        {
            PostalCode = postcode;
            Identificator = new PostinfoIdentificator(naamruimte, postcode, version);
            Status = status?.ConvertFromPostalInformationStatus();
            //MunicipalityNisCode = municipalityNisCode;
            PostalNames = postalNames?
                              .Select(name => new Postnaam(new GeografischeNaam(name.Name, name.Language.ConvertFromLanguage())))
                              .ToList()
                          ?? new List<Postnaam>();
            //Provenance = new Provenance(version, organisation, new Reason(reason));
        }
    }

    public class PostalInformationSyndicationResponseExamples : IExamplesProvider<XmlElement>
    {
        const string RawXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
    <id>https://api.basisregisters.vlaanderen.be/v1/feeds/postinfo.atom</id>
    <title>Basisregisters Vlaanderen - feed 'postinfo'</title>
    <subtitle>Deze Atom feed geeft leestoegang tot events op de resource 'postinfo'.</subtitle>
    <generator uri=""https://basisregisters.vlaanderen.be"" version=""2.2.13.2"">Basisregisters Vlaanderen</generator>
    <rights>Gratis hergebruik volgens https://overheid.vlaanderen.be/sites/default/files/documenten/ict-egov/licenties/hergebruik/modellicentie_gratis_hergebruik_v1_0.html</rights>
    <updated>2020-09-17T09:14:41Z</updated>
    <author>
        <name>agentschap Informatie Vlaanderen</name>
        <email>informatie.vlaanderen@vlaanderen.be</email>
    </author>
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/postinfo"" rel=""self"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/postinfo.atom"" rel=""alternate"" type=""application/atom+xml"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/postinfo.xml"" rel=""alternate"" type=""application/xml"" />
    <link href=""https://docs.basisregisters.vlaanderen.be/"" rel=""related"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/postinfo?from=2&amp;limit=100&amp;embed=event,object"" rel=""next"" />
    <entry>
        <id>0</id>
        <title>PostalInformationWasRegistered-0</title>
        <updated>2019-11-21T09:34:51+01:00</updated>
        <published>2019-11-21T09:34:51+01:00</published>
        <link href=""https://data.vlaanderen.be/id/postinfo/8900"" rel=""related"" />
        <author>
            <name>bpost</name>
        </author>
        <category term=""postinfo"" />
        <content>
            <![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Event><PostalInformationWasRegistered><PostalCode>8900</PostalCode><Provenance><Timestamp>2019-11-21T08:34:51Z</Timestamp><Organisation>DePost</Organisation><Reason>Centrale bijhouding o.b.v. bPost-bestand</Reason></Provenance>
    </PostalInformationWasRegistered>
  </Event><Object><Id>8900</Id><Identificator><Id>https://data.vlaanderen.be/id/postinfo/8900</Id><Naamruimte>https://data.vlaanderen.be/id/postinfo</Naamruimte><ObjectId>8900</ObjectId><VersieId>2019-11-21T09:34:51+01:00</VersieId></Identificator><Postnamen /><NisCode i:nil=""true"" /><PostInfoStatus i:nil=""true"" /><Creatie><Tijdstip>2019-11-21T09:34:51+01:00</Tijdstip><Organisatie>bpost</Organisatie><Reden>Centrale bijhouding o.b.v. bPost-bestand</Reden></Creatie>
  </Object></Content>]]>
</content>
</entry>
<entry>
    <id>1</id>
    <title>PostalInformationBecameCurrent-1</title>
    <updated>2019-11-21T09:34:51+01:00</updated>
    <published>2019-11-21T09:34:51+01:00</published>
    <link href=""https://data.vlaanderen.be/id/postinfo/8900"" rel=""related"" />
    <author>
        <name>bpost</name>
    </author>
    <category term=""postinfo"" />
    <content>
        <![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Event><PostalInformationBecameCurrent><PostalCode>8900</PostalCode><Provenance><Timestamp>2019-11-21T08:34:51Z</Timestamp><Organisation>DePost</Organisation><Reason>Centrale bijhouding o.b.v. bPost-bestand</Reason></Provenance>
    </PostalInformationBecameCurrent>
  </Event><Object><Id>8900</Id><Identificator><Id>https://data.vlaanderen.be/id/postinfo/8900</Id><Naamruimte>https://data.vlaanderen.be/id/postinfo</Naamruimte><ObjectId>8900</ObjectId><VersieId>2019-11-21T09:34:51+01:00</VersieId></Identificator><Postnamen /><NisCode i:nil=""true"" /><PostInfoStatus>Gerealiseerd</PostInfoStatus><Creatie><Tijdstip>2019-11-21T09:34:51+01:00</Tijdstip><Organisatie>bpost</Organisatie><Reden>Centrale bijhouding o.b.v. bPost-bestand</Reden></Creatie>
  </Object></Content>]]>
</content>
</entry>
</feed>";

        public XmlElement GetExamples()
        {
            var example = new XmlDocument();
            example.LoadXml(RawXml);
            return example.DocumentElement;
        }
    }
}

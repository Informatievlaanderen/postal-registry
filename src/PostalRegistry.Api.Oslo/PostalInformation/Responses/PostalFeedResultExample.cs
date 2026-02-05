namespace PostalRegistry.Api.Oslo.PostalInformation.Responses
{
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Filters;

    public sealed class PostalFeedResultExample : IExamplesProvider<object>
    {
        private readonly ResponseOptions _feedConfig;

        public PostalFeedResultExample(IOptions<ResponseOptions> feedConfig)
        {
            _feedConfig = feedConfig.Value;
        }

        public object GetExamples()
        {
            var json = $$"""
                         [
                             {
                                 "specversion": "1.0",
                                 "id": "3534",
                                 "time": "2020-02-10T12:42:50.6472584+01:00",
                                 "type": "basisregisters.postalinformation.create.v1",
                                 "source": "{{_feedConfig.PostalFeed.FeedUrl}}",
                                 "datacontenttype": "application/json",
                                 "dataschema": "{{_feedConfig.PostalFeed.DataSchemaUrl}}",
                                 "basisregisterseventtype": "PostalInformationWasRegistered",
                                 "basisregisterscausationid": "446287de-032c-529b-8338-cd7209051d33",
                                 "data": {
                                     "@id": "https://data.vlaanderen.be/id/postinfo/9000",
                                     "objectId": "9000",
                                     "naamruimte": "https://data.vlaanderen.be/id/postinfo",
                                     "versieId": "2020-02-10T12:42:50.6472584+01:00",
                                     "nisCodes": [],
                                     "attributen": []
                                 }
                             },
                             {
                                 "specversion": "1.0",
                                 "id": "3535",
                                 "time": "2020-02-10T12:42:50.6472584+01:00",
                                 "type": "basisregisters.postalinformation.update.v1",
                                 "source": "{{_feedConfig.PostalFeed.FeedUrl}}",
                                 "datacontenttype": "application/json",
                                 "dataschema": "{{_feedConfig.PostalFeed.DataSchemaUrl}}",
                                 "basisregisterseventtype": "PostalInformationWasRealized",
                                 "basisregisterscausationid": "446287de-032c-529b-8338-cd7209051d33",
                                 "data": {
                                     "@id": "https://data.vlaanderen.be/id/postinfo/9000",
                                     "objectId": "9000",
                                     "naamruimte": "https://data.vlaanderen.be/id/postinfo",
                                     "versieId": "2020-02-10T12:42:50.6472584+01:00",
                                     "nisCodes": [],
                                     "attributen": [
                                         {
                                             "naam": "postInfoStatus",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "gerealiseerd"
                                         }
                                     ]
                                 }
                             }
                         ]
                         """;
            return JArray.Parse(json);
        }
    }
}

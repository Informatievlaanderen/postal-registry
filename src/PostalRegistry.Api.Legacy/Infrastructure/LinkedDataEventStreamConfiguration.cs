using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostalRegistry.Api.Legacy.PostalInformation
{
    public class LinkedDataEventStreamConfiguration
    {
        public string DataVlaanderenNamespace { get; set; }
        public string ApiEndpoint { get; set; }
        public LinkedDataEventStreamConfiguration(IConfigurationSection configuration)
        {
            DataVlaanderenNamespace = configuration["DataVlaanderenNamespace"];
            ApiEndpoint = configuration["ApiEndpoint"];
        }
    }
}
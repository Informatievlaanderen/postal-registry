namespace PostalRegistry.Projections.Syndication.Municipality
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;

    [DataContract(Name = "Gemeente", Namespace = "")]
    public class Gemeente
    {
        [DataMember(Name = "Id", Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Name = "Identificator", Order = 2)]
        public Identificator Identificator { get; set; }

        [DataMember(Name = "Gemeentenamen", Order = 3)]
        public List<GeografischeNaam> Gemeentenamen { get; set; }

        [DataMember(Name = "GemeenteStatus", Order = 4)]
        public GemeenteStatus? GemeenteStatus { get; set; }

        public Gemeente()
        {
            Gemeentenamen = new List<GeografischeNaam>();
        }
    }
}

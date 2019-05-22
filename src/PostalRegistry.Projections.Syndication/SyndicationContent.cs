namespace PostalRegistry.Projections.Syndication
{
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name = "Content", Namespace = "")]
    public class SyndicationContent<T>
    {
        [DataMember(Name = "Event")]
        public XmlElement Event { get; set; }

        [DataMember(Name = "Object")]
        public T Object { get; set; }
    }
}

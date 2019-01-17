namespace PostalRegistry.DataStructures.BPost
{
    public class PostalNameData
    {
        public string Name { get; set; }

        public Language Language { get; set; }

        public PostalNameData() { }

        public PostalNameData(string name, Language language)
        {
            Name = name;
            Language = language;
        }
    }
}

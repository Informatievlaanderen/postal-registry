namespace PostalRegistry.Api.Extract.Extracts.ExtractFiles
{
    using System;

    public class ExtractFileName
    {
        private readonly string _name;
        private readonly string _extension;

        public ExtractFileName(string name, string extension)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentNullException(nameof(extension));

            _name = name;
            _extension = extension;
        }

        public override string ToString()
        {
            var extension = _extension.Trim();
            if (false == extension.StartsWith('.'))
                extension = "." + extension;

            var name = _name
                .Trim()
                .TrimEnd('.');
            var nameIncludesExtension = name.ToLowerInvariant().EndsWith(extension.ToLowerInvariant());

            return nameIncludesExtension ? name : name + extension;
        }

        public static implicit operator string(ExtractFileName name)
        {
            if (null == name)
                throw new ArgumentNullException(nameof(name));

            return name.ToString();
        }
    }

    public class DbfFileName : ExtractFileName
    {
        public DbfFileName(string name)
            : base(name, "dbf")
        { }
    }

    public class ShpFileName : ExtractFileName
    {
        public ShpFileName(string name)
            : base(name, "shp")
        { }
    }

    public class ShxFileName : ExtractFileName
    {
        public ShxFileName(string name)
            : base(name, "shx")
        { }
    }
}

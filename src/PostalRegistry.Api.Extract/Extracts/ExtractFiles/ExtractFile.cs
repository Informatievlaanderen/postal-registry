namespace PostalRegistry.Api.Extract.Extracts.ExtractFiles
{
    using System;
    using System.IO;
    using System.Threading;

    public class ExtractFile
    {
        private readonly Action<Stream, CancellationToken> _writeFile;
        public ExtractFileName Name { get; }

        public ExtractFile(ExtractFileName name, Action<Stream, CancellationToken> writeFile)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _writeFile = writeFile ?? throw new ArgumentNullException(nameof(writeFile));
        }

        public void WriteTo(Stream writeStream, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            _writeFile(writeStream, token);
        }

    }
}

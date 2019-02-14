namespace PostalRegistry.Api.Extract.Extracts.ExtractFiles
{
    using System;
    using System.IO;
    using System.Text;

    public abstract class ExtractFileWriter : IDisposable
    {
        protected readonly BinaryWriter Writer;

        protected ExtractFileWriter(Encoding encoding, Stream contentStream)
        {
            if (null == encoding)
                throw new ArgumentNullException(nameof(encoding));

            if (contentStream == null)
                throw new ArgumentNullException(nameof(contentStream));

            Writer = new BinaryWriter(contentStream, encoding, leaveOpen: true);
        }

        public void Dispose()
        {
            Writer.Dispose();
        }
    }
}

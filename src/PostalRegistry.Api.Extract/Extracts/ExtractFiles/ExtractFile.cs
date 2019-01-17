namespace PostalRegistry.Api.Extract.Extracts.ExtractFiles
{
    using System;
    using System.IO;
    using System.Text;

    public abstract class ExtractFile : IDisposable
    {
        protected readonly BinaryWriter Writer;

        private readonly string _name;
        private readonly Stream _content;

        protected ExtractFile(string name, string extension, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentNullException(nameof(extension));

            if (null == encoding)
                throw new ArgumentNullException(nameof(encoding));

            if (false == extension.StartsWith('.'))
                extension = '.' + extension;

            var nameIncludesExtension = name.ToLowerInvariant().EndsWith(extension.ToLowerInvariant());
            _name = nameIncludesExtension ? name : name.TrimEnd('.') + extension;
            _content = new MemoryStream();
            Writer = new BinaryWriter(_content, encoding);
        }

        protected abstract void BeforeFlush();

        public FileFlushResult Flush()
        {
            BeforeFlush();

            Writer.Flush();
            _content.Position = 0;
            return new FileFlushResult(_name, _content);
        }

        public void Dispose()
        {
            Writer.Dispose();
            _content.Dispose();
        }

        public class FileFlushResult
        {
            internal FileFlushResult(string name, Stream content)
            {
                Name = name;
                Content = content;
            }

            public string Name { get; }
            public Stream Content { get; }
        }
    }
}

namespace PostalRegistry.Api.Extract.Extracts.ExtractFiles
{
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using System.IO;
    using System.Text;

    public class DbfFileWriter<TDbaseRecord> : ExtractFileWriter
        where TDbaseRecord : DbaseRecord
    {
        private static Encoding Encoding => Encoding.GetEncoding(1252);


        public DbfFileWriter(DbaseFileHeader header, Stream writeStream)
            : base(Encoding, writeStream)
        {
            header.Write(Writer);
        }

        public void Write(TDbaseRecord record)
        {
            record.Write(Writer);
        }

        public void WriteBytesAs<T>(byte[] recordBytes)
            where T : TDbaseRecord, new()
        {
            var record = new T();
            record.FromBytes(recordBytes, Encoding);
            Write(record);
        }

        public void WriteEndOfFile()
        {
            Writer.Write(DbaseRecord.EndOfFile);
        }
    }
}

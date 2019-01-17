namespace PostalRegistry.Api.Extract.Extracts.ExtractFiles
{
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class DbfFile<TDbaseRecord> : ExtractFile
        where TDbaseRecord : DbaseRecord
    {
        private static Encoding Encoding => DbaseCodePage.Western_European_ANSI.ToEncoding();

        public DbfFile(string name, DbaseFileHeader header)
            : base(name, "dbf", Encoding) => header.Write(Writer);

        public void Write(IEnumerable<TDbaseRecord> records)
        {
            foreach (var record in records)
                record.Write(Writer);
        }

        public void WriteBytesAs<T>(IEnumerable<byte[]> records)
            where T : TDbaseRecord, new()
            => Write(records.Select(CreateRecordFromBytes<T>));


        private static T CreateRecordFromBytes<T>(byte[] record)
            where T : TDbaseRecord, new()
        {
            var dbaseRecord = new T();
            dbaseRecord.FromBytes(record, Encoding);
            return dbaseRecord;
        }

        protected sealed override void BeforeFlush() => Writer.Write(DbaseRecord.EndOfFile);
    }
}

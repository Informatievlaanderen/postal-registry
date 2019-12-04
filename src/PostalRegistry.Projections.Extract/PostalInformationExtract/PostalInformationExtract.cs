namespace PostalRegistry.Projections.Extract.PostalInformationExtract
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PostalInformationExtractItem
    {
        public string PostalCode { get; set; }
        public byte[] DbaseRecord { get; set; }
    }

    public class PostalExtractItemConfiguration : IEntityTypeConfiguration<PostalInformationExtractItem>
    {
        private const string TableName = "Postal";

        public void Configure(EntityTypeBuilder<PostalInformationExtractItem> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => p.PostalCode)
                .ForSqlServerIsClustered(false);

            builder.Property(p => p.DbaseRecord);

            builder.HasIndex(p => p.PostalCode).ForSqlServerIsClustered();
        }
    }
}

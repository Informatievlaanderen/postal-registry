namespace PostalRegistry.Projections.Extract.PostalInformationExtract
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PostalInformationExtractItem
    {
        public string PostalCode { get; set; }
        public string PostName { get; set; }
        public byte[] DbaseRecord { get; set; }
    }

    public class PostalExtractItemConfiguration : IEntityTypeConfiguration<PostalInformationExtractItem>
    {
        public const string TableName = "Postal";

        public void Configure(EntityTypeBuilder<PostalInformationExtractItem> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => new { p.PostalCode, p.PostName })
                .ForSqlServerIsClustered(false);

            builder.Property(p => p.DbaseRecord);

            builder.HasIndex(p => p.PostalCode).ForSqlServerIsClustered();
        }
    }
}

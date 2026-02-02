namespace PostalRegistry.Projections.Feed.PostalFeed
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PostalFeedItem
    {
        public long Id { get; set; }
        public int Page { get; set; }
        public long Position { get; set; }

        public string PostalCode { get; set; } = null!;
        public string? NisCode { get; set; }

        public Application? Application { get; set; }
        public Modification? Modification { get; set; }
        public string? Operator { get; set; }
        public Organisation? Organisation { get; set; }
        public string? Reason { get; set; }
        public string CloudEventAsString { get; set; } = null!;

        private PostalFeedItem() { }

        public PostalFeedItem(long position, int page, string postalCode) : this()
        {
            PostalCode = postalCode;
            Page = page;
            Position = position;
        }
    }

    public class PostalFeedConfiguration : IEntityTypeConfiguration<PostalFeedItem>
    {
        private const string TableName = "PostalFeed";

        public void Configure(EntityTypeBuilder<PostalFeedItem> b)
        {
            b.ToTable(TableName, Schema.Feed)
                .HasKey(x => x.Id)
                .IsClustered();

            b.Property(x => x.Id)
                .UseHiLo("PostalFeedSequence", Schema.Feed);

            b.Property(x => x.CloudEventAsString)
                .HasColumnName("CloudEvent")
                .IsRequired();

            b.Property(x => x.PostalCode).HasMaxLength(4).IsRequired();
            b.Property(x => x.NisCode).HasMaxLength(5);

            b.Property(x => x.Application);
            b.Property(x => x.Modification);
            b.Property(x => x.Operator);
            b.Property(x => x.Organisation);
            b.Property(x => x.Reason);

            b.HasIndex(x => x.Position);
            b.HasIndex(x => x.Page);
            b.HasIndex(x => x.PostalCode);
        }
    }
}

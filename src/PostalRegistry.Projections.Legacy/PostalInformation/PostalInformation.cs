namespace PostalRegistry.Projections.Legacy.PostalInformation
{
    using System;
    using System.Collections.Generic;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class PostalInformation
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public string PostalCode { get; set; }
        public string? NisCode { get; set; }

        public bool IsRetired { get; set; }

        public List<PostalInformationName> PostalNames { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public bool IsRemoved { get; set; }
    }

    public class PostalInformationConfiguration : IEntityTypeConfiguration<PostalInformation>
    {
        private const string TableName = "PostalInformation";

        public void Configure(EntityTypeBuilder<PostalInformation> builder)
        {
            builder.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.PostalCode)
                .IsClustered();

            builder.Property(p => p.NisCode);

            builder.Property(p => p.IsRetired);
            builder.Property(p => p.IsRemoved)
                .HasDefaultValue(false);

            builder.HasMany(p => p.PostalNames)
                .WithOne()
                .HasForeignKey(p => p.PostalCode);

            builder.Property(PostalInformation.VersionTimestampBackingPropertyName)
                .HasColumnName("VersionTimestamp");

            builder.Ignore(p => p.VersionTimestamp);

            builder.HasIndex(x => x.NisCode);
            builder.HasIndex(x => x.IsRemoved);
        }
    }
}

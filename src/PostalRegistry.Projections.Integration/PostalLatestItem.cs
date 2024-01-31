namespace PostalRegistry.Projections.Integration
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using PostalRegistry.Infrastructure;

    public sealed class PostalLatestItem
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public string PostalCode { get; set; }
        public string? NisCode { get; set; }
        public bool IsRetired { get; set; }
        public List<PostalInformationName> PostalNames { get; set; }
        public string? PuriId { get; set; }
        public string? Namespace { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
        public PostalLatestItem()
        { }
    }

    public sealed class MunicipalityLatestItemConfiguration : IEntityTypeConfiguration<PostalLatestItem>
    {
        public void Configure(EntityTypeBuilder<PostalLatestItem> builder)
        {
            builder
                .ToTable("postal_latest_items", Schema.Integration)
                .HasKey(x => x.PostalCode);

            builder.Property(p => p.PostalCode)
                .HasColumnName("postal_code");

            builder.Property(p => p.NisCode)
                .HasColumnName("nis_code");

            builder.Property(p => p.IsRetired)
                .HasColumnName("is_retired");

            builder.HasMany(p => p.PostalNames)
                .WithOne()
                .HasForeignKey(p => p.PostalCode);

            builder.Property(PostalLatestItem.VersionTimestampBackingPropertyName)
                .HasColumnName("version_timestamp");

            builder.Property(x => x.PuriId).HasColumnName("puri_id");
            builder.Property(x => x.Namespace).HasColumnName("namespace");

            builder.Ignore(p => p.VersionTimestamp);

            builder.HasIndex(x => x.NisCode);
        }
    }
}

namespace PostalRegistry.Producer.Ldes
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using PostalRegistry.Infrastructure;

    public class PostalInformationDetail
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);
        public const string NamesDutchBackingPropertyName = nameof(_namesDutchBackingProperty);
        public const string NamesFrenchBackingPropertyName = nameof(_namesFrenchBackingProperty);
        public const string NamesEnglishBackingPropertyName = nameof(_namesEnglishBackingProperty);
        public const string NamesGermanBackingPropertyName = nameof(_namesGermanBackingProperty);

        private const char NamesSeparator = '|';

        public string PostalCode { get; set; }
        public string? NisCode { get; set; }

        public bool IsRetired { get; set; }

        private string? _namesDutchBackingProperty;
        public ICollection<string> NamesDutch
        {
            get => _namesDutchBackingProperty?.Split(NamesSeparator) ?? [];
            set => _namesDutchBackingProperty = string.Join(NamesSeparator, value);
        }

        private string? _namesFrenchBackingProperty;
        public ICollection<string> NamesFrench
        {
            get => _namesFrenchBackingProperty?.Split(NamesSeparator) ?? [];
            set => _namesFrenchBackingProperty = string.Join(NamesSeparator, value);
        }

        private string? _namesEnglishBackingProperty;
        public ICollection<string> NamesEnglish
        {
            get => _namesEnglishBackingProperty?.Split(NamesSeparator) ?? [];
            set => _namesEnglishBackingProperty = string.Join(NamesSeparator, value);
        }

        private string? _namesGermanBackingProperty;
        public ICollection<string> NamesGerman
        {
            get => _namesGermanBackingProperty?.Split(NamesSeparator) ?? [];
            set => _namesGermanBackingProperty = string.Join(NamesSeparator, value);
        }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public bool IsRemoved { get; set; }
    }

    public class PostalInformationDetailConfiguration : IEntityTypeConfiguration<PostalInformationDetail>
    {
        private const string TableName = "PostalInformation";

        public void Configure(EntityTypeBuilder<PostalInformationDetail> builder)
        {
            builder.ToTable(TableName, Schema.ProducerLdes)
                .HasKey(p => p.PostalCode)
                .IsClustered();

            builder.Property(p => p.NisCode);
            builder.Property(p => p.IsRetired);

            builder.Property(PostalInformationDetail.NamesDutchBackingPropertyName)
                .HasColumnName(nameof(PostalInformationDetail.NamesDutch));
            builder.Ignore(p => p.NamesDutch);

            builder.Property(PostalInformationDetail.NamesFrenchBackingPropertyName)
                .HasColumnName(nameof(PostalInformationDetail.NamesFrench));
            builder.Ignore(p => p.NamesFrench);

            builder.Property(PostalInformationDetail.NamesEnglishBackingPropertyName)
                .HasColumnName(nameof(PostalInformationDetail.NamesEnglish));
            builder.Ignore(p => p.NamesEnglish);

            builder.Property(PostalInformationDetail.NamesGermanBackingPropertyName)
                .HasColumnName(nameof(PostalInformationDetail.NamesGerman));
            builder.Ignore(p => p.NamesGerman);

            builder.Property(PostalInformationDetail.VersionTimestampBackingPropertyName)
                .HasColumnName(nameof(PostalInformationDetail.VersionTimestamp));
            builder.Ignore(p => p.VersionTimestamp);

            builder.Property(x => x.IsRemoved)
                .HasDefaultValue(false);

            builder.HasIndex(x => x.NisCode);
        }
    }
}

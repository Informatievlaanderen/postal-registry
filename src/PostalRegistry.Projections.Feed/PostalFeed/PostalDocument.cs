namespace PostalRegistry.Projections.Feed.PostalFeed
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Newtonsoft.Json;
    using NodaTime;

    public class PostalDocument
    {
        public string PostalCode { get; set; } = null!;
        public DateTimeOffset LastChangedOnAsDateTimeOffset { get; set; }
        public DateTimeOffset RecordCreatedAtAsDateTimeOffset { get; set; }

        public Instant RecordCreatedAt
        {
            get => Instant.FromDateTimeOffset(RecordCreatedAtAsDateTimeOffset);
            set => RecordCreatedAtAsDateTimeOffset = value.ToBelgianDateTimeOffset();
        }

        public Instant LastChangedOn
        {
            get => Instant.FromDateTimeOffset(LastChangedOnAsDateTimeOffset);
            set
            {
                var belgianDateTimeOffset = value.ToBelgianDateTimeOffset();
                LastChangedOnAsDateTimeOffset = belgianDateTimeOffset;
                Document.VersionId = belgianDateTimeOffset;
            }
        }

        public PostalJsonDocument Document { get; set; }

        private PostalDocument()
        { }

        public PostalDocument(
            string postalCode,
            Instant createdTimestamp) : this()
        {
            PostalCode = postalCode;
            RecordCreatedAt = createdTimestamp;

            Document = new PostalJsonDocument
            {
                PostalCode = postalCode,
            };

            LastChangedOn = createdTimestamp;
        }
    }

    public sealed record PostalJsonDocument
    {
        public string PostalCode { get; set; } = null!;
        public string? NisCode { get; set; }
        public PostInfoStatus? Status { get; set; }
        public DateTimeOffset VersionId { get; set; }
        public List<GeografischeNaam> Names { get; set; } = [];
    }

    public sealed class PostalDocumentConfiguration : IEntityTypeConfiguration<PostalDocument>
    {
        private readonly JsonSerializerSettings _serializerSettings;
        private const string TableName = "PostalDocuments";

        public PostalDocumentConfiguration(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public void Configure(EntityTypeBuilder<PostalDocument> b)
        {
            b.ToTable(TableName, Schema.Feed)
                .HasKey(x => x.PostalCode)
                .IsClustered(false);

            b.Property(x => x.PostalCode)
                .HasMaxLength(5);

            b.Property(x => x.Document)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, _serializerSettings),
                    v => JsonConvert.DeserializeObject<PostalJsonDocument>(v, _serializerSettings) ?? new PostalJsonDocument());

            b.Property(x => x.RecordCreatedAtAsDateTimeOffset).HasColumnName("RecordCreatedAt");
            b.Property(x => x.LastChangedOnAsDateTimeOffset).HasColumnName("LastChangedOn");

            b.Ignore(x => x.LastChangedOn);
            b.Ignore(x => x.RecordCreatedAt);

            b.HasIndex(x => x.PostalCode);
        }
    }
}

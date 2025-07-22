namespace PostalRegistry.Projections.Legacy.PostalInformationSyndication
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Newtonsoft.Json;
    using NodaTime;

    public class PostalInformationSyndicationItem
    {
        public long Position { get; set; }

        public string? PostalCode { get; set; }
        public string? ChangeType { get; set; }

        public PostalInformationStatus? Status { get; set; }

        public string? MunicipalityNisCode { get; set; }

        public string? PostalNamesAsJson { get; set; }

        public IReadOnlyList<PostalName> PostalNames => GetPostalNamesAsList();

        public DateTimeOffset RecordCreatedAtAsDateTimeOffset { get; set; }
        public DateTimeOffset LastChangedOnAsDateTimeOffset { get; set; }

        public Instant RecordCreatedAt
        {
            get => Instant.FromDateTimeOffset(RecordCreatedAtAsDateTimeOffset);
            set => RecordCreatedAtAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public Instant LastChangedOn
        {
            get => Instant.FromDateTimeOffset(LastChangedOnAsDateTimeOffset);
            set => LastChangedOnAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public Application? Application { get; set; }
        public Modification? Modification { get; set; }
        public string? Operator { get; set; }
        public Organisation? Organisation { get; set; }
        public string? Reason { get; set; }
        public string? EventDataAsXml { get; set; }
        public DateTimeOffset SyndicationItemCreatedAt { get; set; }
        public bool IsRemoved { get; set; }

        public PostalInformationSyndicationItem CloneAndApplyEventInfo(
            long newPosition,
            string eventName,
            Instant lastChangedOn,
            Action<PostalInformationSyndicationItem> editFunc)
        {
            var newItem = new PostalInformationSyndicationItem
            {
                Position = newPosition,

                PostalCode = PostalCode,
                ChangeType = eventName,

                Status = Status,
                MunicipalityNisCode = MunicipalityNisCode,

                PostalNamesAsJson = PostalNamesAsJson,

                RecordCreatedAt = RecordCreatedAt,
                LastChangedOn = lastChangedOn,

                Application = Application,
                Modification = Modification,
                Operator = Operator,
                Organisation = Organisation,
                Reason = Reason,
                SyndicationItemCreatedAt = DateTimeOffset.UtcNow
            };

            editFunc(newItem);

            return newItem;
        }

        public void AddPostalName(PostalName postalName)
        {
            var newList = GetPostalNamesAsList();
            newList.Add(postalName);
            PostalNamesAsJson = JsonConvert.SerializeObject(newList);
        }

        public void RemovePostalName(PostalName postalName)
        {
            var newList = GetPostalNamesAsList();
            newList.Remove(postalName);
            PostalNamesAsJson = JsonConvert.SerializeObject(newList);
        }

        private List<PostalName> GetPostalNamesAsList()
            => string.IsNullOrEmpty(PostalNamesAsJson)
                ? new List<PostalName>()
                : JsonConvert.DeserializeObject<List<PostalName>>(PostalNamesAsJson);
    }

    public class PostalInformationSyndicationConfiguration : IEntityTypeConfiguration<PostalInformationSyndicationItem>
    {
        private const string TableName = "PostalInformationSyndication";

        public void Configure(EntityTypeBuilder<PostalInformationSyndicationItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(x => x.Position)
                .IsClustered();

            b.Property(x => x.Position).ValueGeneratedNever();
            b.HasIndex(x => x.Position).IsColumnStore($"CI_{TableName}_Position");

            b.Property(x => x.PostalCode).IsRequired();
            b.Property(x => x.ChangeType);

            b.Property(x => x.Status);
            b.Property(x => x.MunicipalityNisCode);

            b.Property(x => x.RecordCreatedAtAsDateTimeOffset).HasColumnName("RecordCreatedAt");
            b.Property(x => x.LastChangedOnAsDateTimeOffset).HasColumnName("LastChangedOn");
            b.Property(x => x.PostalNamesAsJson).HasColumnName("PostalNames");

            b.Property(x => x.Application);
            b.Property(x => x.Modification);
            b.Property(x => x.Operator);
            b.Property(x => x.Organisation);
            b.Property(x => x.Reason);
            b.Property(x => x.EventDataAsXml);
            b.Property(x => x.SyndicationItemCreatedAt).IsRequired();
            b.Property(x => x.IsRemoved);

            b.Ignore(x => x.PostalNames);
            b.Ignore(x => x.RecordCreatedAt);
            b.Ignore(x => x.LastChangedOn);

            b.HasIndex(x => x.PostalCode);
            b.HasIndex(x => x.IsRemoved);
        }
    }
}

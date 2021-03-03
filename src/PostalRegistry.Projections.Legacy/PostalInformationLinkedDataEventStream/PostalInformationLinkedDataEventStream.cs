using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using NodaTime;
using PostalRegistry.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostalRegistry.Projections.Legacy.PostalInformationLinkedDataEventStream
{
    public class PostalInformationLinkedDataEventStreamItem
    {
        public long Position { get; set; }

        public string? PostalCode { get; set; }
        public string? ChangeType { get; set; }

        public PostalInformationStatus? Status { get; set; }

        public string? PostalNamesAsJson { get; set; }

        public IReadOnlyList<PostalName> PostalNames => GetPostalNamesAsList();

        public DateTimeOffset RecordCreatedAtAsDateTimeOffset { get; set; }

        public Instant RecordCreatedAt
        {
            get => Instant.FromDateTimeOffset(RecordCreatedAtAsDateTimeOffset);
            set => RecordCreatedAtAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public PostalInformationLinkedDataEventStreamItem CloneAndApplyEventInfo(
            long newPosition,
            string eventName,
            Action<PostalInformationLinkedDataEventStreamItem> editFunc)
        {
            var newItem = new PostalInformationLinkedDataEventStreamItem
            {
                Position = newPosition,

                PostalCode = PostalCode,
                ChangeType = eventName,

                Status = Status,

                PostalNamesAsJson = PostalNamesAsJson,

                RecordCreatedAt = RecordCreatedAt
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

    public class PostalInformationLinkedDataEventStreamConfiguration : IEntityTypeConfiguration<PostalInformationLinkedDataEventStreamItem>
    {
        private const string TableName = "PostalInformationLinkedDataEventStream";

        public void Configure(EntityTypeBuilder<PostalInformationLinkedDataEventStreamItem> builder)
        {
            builder.ToTable(TableName, Schema.Legacy)
                .HasKey(x => x.Position)
                .IsClustered();

            builder.Property(x => x.Position).ValueGeneratedNever();
            builder.HasIndex(x => x.Position).IsColumnStore($"CI_{TableName}_Position");

            builder.Property(x => x.PostalCode).IsRequired();
            builder.Property(x => x.ChangeType);

            builder.Property(x => x.Status);

            builder.Property(x => x.PostalNamesAsJson).HasColumnName("PostalNames");
            builder.Property(x => x.RecordCreatedAtAsDateTimeOffset).HasColumnName("RecordCreatedAt");

            builder.Ignore(x => x.PostalNames);
            builder.Ignore(x => x.RecordCreatedAt);

            builder.HasIndex(x => x.PostalCode);
            
        }
    }
}

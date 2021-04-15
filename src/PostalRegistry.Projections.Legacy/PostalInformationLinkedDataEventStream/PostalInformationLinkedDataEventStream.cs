namespace PostalRegistry.Projections.Legacy.PostalInformationLinkedDataEventStream
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Newtonsoft.Json;
    using NodaTime;
    using Infrastructure;
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    public class PostalInformationLinkedDataEventStreamItem
    {
        public long Position { get; set; }

        public string? PostalCode { get; set; }
        public string? ChangeType { get; set; }

        public PostalInformationStatus? Status { get; set; }

        public string? PostalNamesAsJson { get; set; }

        public IReadOnlyList<PostalName> PostalNames => GetPostalNamesAsList();

        public DateTimeOffset EventGeneratedAtTimeAsDatetimeOffset { get; set; }

        public Instant EventGeneratedAtTime
        {
            get => Instant.FromDateTimeOffset(EventGeneratedAtTimeAsDatetimeOffset);
            set => EventGeneratedAtTimeAsDatetimeOffset = value.ToDateTimeOffset();
        }

        public string ObjectHash { get; private set; }

        public PostalInformationLinkedDataEventStreamItem CloneAndApplyEventInfo(
            long newPosition,
            string eventName,
            Instant generatedAtTime,
            Action<PostalInformationLinkedDataEventStreamItem> editFunc)
        {
            var newItem = new PostalInformationLinkedDataEventStreamItem
            {
                Position = newPosition,

                PostalCode = PostalCode,
                ChangeType = eventName,

                Status = Status,

                PostalNamesAsJson = PostalNamesAsJson,

                EventGeneratedAtTime = generatedAtTime
            };

            editFunc(newItem);
            newItem.SetObjectHash();

            return newItem;
        }

        public void SetObjectHash()
        {
            ObjectHash = string.Empty;
            var objectString = JsonConvert.SerializeObject(this);

            using var md5Hash = MD5.Create();
            var hashBytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(objectString));
            ObjectHash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
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
        private const string TableName = "PostalInformation";

        public void Configure(EntityTypeBuilder<PostalInformationLinkedDataEventStreamItem> builder)
        {
            builder.ToTable(TableName, Schema.LinkedDataEventStream)
                .HasKey(x => x.Position)
                .IsClustered();

            builder.Property(x => x.Position).ValueGeneratedNever();
            builder.HasIndex(x => x.Position).IsColumnStore($"CI_{TableName}_Position");

            builder.Property(x => x.PostalCode).IsRequired();
            builder.Property(x => x.ChangeType);

            builder.Property(x => x.Status);

            builder.Property(x => x.PostalNamesAsJson).HasColumnName("PostalNames");
            builder.Property(x => x.EventGeneratedAtTimeAsDatetimeOffset).HasColumnName("EventGeneratedAtTime");
            builder.Property(x => x.ObjectHash).HasColumnName("ObjectIdentifier");

            builder.Ignore(x => x.PostalNames);
            builder.Ignore(x => x.EventGeneratedAtTime);

            builder.HasIndex(x => x.PostalCode);
        }
    }
}

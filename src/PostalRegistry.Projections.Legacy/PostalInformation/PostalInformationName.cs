namespace PostalRegistry.Projections.Legacy.PostalInformation
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PostalInformationName
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SearchName { get; set; }
        public Language Language { get; set; }
        public string? PostalCode { get; set; }

        public PostalInformationName() { }

        public PostalInformationName(string name, string searchName, string postalCode, Language language)
        {
            Name = name;
            SearchName = searchName;
            PostalCode = postalCode;
            Language = language;
        }
    }

    public class PostalInformationNameConfiguration : IEntityTypeConfiguration<PostalInformationName>
    {
        private const string TableName = "PostalInformationName";

        public void Configure(EntityTypeBuilder<PostalInformationName> builder)
        {
            builder.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.Id)
                .IsClustered();

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            builder.Property(p => p.Name).IsRequired();
            builder.Property(p => p.SearchName).IsRequired();
            builder.Property(p => p.PostalCode);
            builder.Property(p => p.Language);

            builder.HasIndex(x => x.PostalCode);
            builder.HasIndex(x => x.SearchName);
        }
    }
}

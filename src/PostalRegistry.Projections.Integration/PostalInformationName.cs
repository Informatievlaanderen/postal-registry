namespace PostalRegistry.Projections.Integration
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using PostalRegistry.Infrastructure;

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
        public void Configure(EntityTypeBuilder<PostalInformationName> builder)
        {
            builder.ToTable("postal_information_name", Schema.Integration)
                .HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");;

            builder.Property(p => p.Name)
                .IsRequired()
                .HasColumnName("name");;
            builder.Property(p => p.SearchName)
                .IsRequired()
                .HasColumnName("search_name");;
            builder.Property(p => p.PostalCode)
                .HasColumnName("postal_code");;
            builder.Property(p => p.Language)
                .HasColumnName("language");;

            builder.HasIndex(x => x.PostalCode);
            builder.HasIndex(x => x.SearchName);
        }
    }
}

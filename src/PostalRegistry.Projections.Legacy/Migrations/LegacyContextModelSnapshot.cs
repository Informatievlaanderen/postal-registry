﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PostalRegistry.Projections.Legacy;

namespace PostalRegistry.Projections.Legacy.Migrations
{
    [DbContext(typeof(LegacyContext))]
    partial class LegacyContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DesiredState")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("DesiredStateChangedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.HasKey("Name")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("ProjectionStates","PostalRegistryLegacy");
                });

            modelBuilder.Entity("PostalRegistry.Projections.Legacy.PostalInformation.PostalInformation", b =>
                {
                    b.Property<string>("PostalCode")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsRetired")
                        .HasColumnType("bit");

                    b.Property<string>("NisCode")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("VersionTimestamp")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("PostalCode")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("NisCode");

                    b.ToTable("PostalInformation","PostalRegistryLegacy");
                });

            modelBuilder.Entity("PostalRegistry.Projections.Legacy.PostalInformation.PostalInformationName", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Language")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PostalCode")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("PostalCode");

                    b.ToTable("PostalInformationName","PostalRegistryLegacy");
                });

            modelBuilder.Entity("PostalRegistry.Projections.Legacy.PostalInformationSyndication.PostalInformationSyndicationItem", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.Property<int?>("Application")
                        .HasColumnType("int");

                    b.Property<string>("ChangeType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EventDataAsXml")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("LastChangedOnAsDateTimeOffset")
                        .HasColumnName("LastChangedOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("Modification")
                        .HasColumnType("int");

                    b.Property<string>("MunicipalityNisCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Operator")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Organisation")
                        .HasColumnType("int");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("PostalNamesAsJson")
                        .HasColumnName("PostalNames")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("RecordCreatedAtAsDateTimeOffset")
                        .HasColumnName("RecordCreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.HasKey("Position")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("Position")
                        .HasName("CI_PostalInformationSyndication_Position")
                        .HasAnnotation("SqlServer:ColumnStoreIndex", "");

                    b.HasIndex("PostalCode");

                    b.ToTable("PostalInformationSyndication","PostalRegistryLegacy");
                });

            modelBuilder.Entity("PostalRegistry.Projections.Legacy.PostalInformation.PostalInformationName", b =>
                {
                    b.HasOne("PostalRegistry.Projections.Legacy.PostalInformation.PostalInformation", null)
                        .WithMany("PostalNames")
                        .HasForeignKey("PostalCode");
                });
#pragma warning restore 612, 618
        }
    }
}

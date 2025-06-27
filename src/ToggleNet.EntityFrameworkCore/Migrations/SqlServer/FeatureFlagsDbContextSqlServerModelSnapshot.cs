using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ToggleNet.EntityFrameworkCore;

namespace ToggleNet.EntityFrameworkCore.Migrations.SqlServer
{
    [DbContext(typeof(FeatureFlagsDbContext))]
    partial class FeatureFlagsDbContextSqlServerModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.32")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ToggleNet.Core.Entities.FeatureFlag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(500)")
                        .HasMaxLength(500);

                    b.Property<string>("Environment")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<int>("RolloutPercentage")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("ToggleNet_FeatureFlags");
                });

            modelBuilder.Entity("ToggleNet.Core.Entities.FeatureUsage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AdditionalData")
                        .HasColumnType("nvarchar(4000)")
                        .HasMaxLength(4000);

                    b.Property<string>("Environment")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("FeatureName")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.HasIndex("Environment");

                    b.HasIndex("FeatureName");

                    b.HasIndex("Timestamp");

                    b.HasIndex("UserId");

                    b.HasIndex("FeatureName", "Environment");

                    b.HasIndex("FeatureName", "UserId", "Environment");

                    b.ToTable("ToggleNet_FeatureUsages");
                });
#pragma warning restore 612, 618
        }
    }
}

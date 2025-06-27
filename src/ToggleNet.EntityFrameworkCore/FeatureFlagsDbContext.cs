using Microsoft.EntityFrameworkCore;
using ToggleNet.Core.Entities;

namespace ToggleNet.EntityFrameworkCore
{
    /// <summary>
    /// DbContext for managing feature flags
    /// </summary>
    public class FeatureFlagsDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the FeatureFlagsDbContext
        /// </summary>
        /// <param name="options">The DbContext options</param>
        public FeatureFlagsDbContext(DbContextOptions<FeatureFlagsDbContext> options) 
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the feature flags
        /// </summary>
        public DbSet<FeatureFlag> FeatureFlags { get; set; }
        
        /// <summary>
        /// Gets or sets the feature usage tracking data
        /// </summary>
        public DbSet<FeatureUsage> FeatureUsages { get; set; }

        /// <summary>
        /// Configures the model for feature flags
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Get the database provider name to make provider-specific configurations
            string? provider = Database.ProviderName;
            bool isPostgres = provider?.Contains("Npgsql") ?? false;
            
            var entity = modelBuilder.Entity<FeatureFlag>();
            
            // Configure table name (schema varies by provider)
            if (isPostgres)
            {
                // PostgreSQL supports schemas well
                entity.ToTable("FeatureFlags", "togglenet");
            }
            else
            {
                // For SQL Server and others, either use dbo schema or no schema
                entity.ToTable("ToggleNet_FeatureFlags");
            }
                
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.Name)
                .IsUnique();
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            
            entity.Property(e => e.Environment)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.RolloutPercentage)
                .HasDefaultValue(0);
                
            // Configure FeatureUsage entity
            var usageEntity = modelBuilder.Entity<FeatureUsage>();
            
            // Configure table name (schema varies by provider)
            if (isPostgres)
            {
                usageEntity.ToTable("FeatureUsages", "togglenet");
            }
            else
            {
                usageEntity.ToTable("ToggleNet_FeatureUsages");
            }
            
            usageEntity.HasKey(e => e.Id);
            
            // Create indexes for common query patterns
            usageEntity.HasIndex(e => e.FeatureName);
            usageEntity.HasIndex(e => e.UserId);
            usageEntity.HasIndex(e => e.Environment);
            usageEntity.HasIndex(e => e.Timestamp);
            
            // Composite indexes for improved query performance
            usageEntity.HasIndex(e => new { e.FeatureName, e.Environment });
            usageEntity.HasIndex(e => new { e.FeatureName, e.UserId, e.Environment });
            
            usageEntity.Property(e => e.Id)
                .ValueGeneratedOnAdd();
                
            usageEntity.Property(e => e.FeatureName)
                .IsRequired()
                .HasMaxLength(100);
                
            usageEntity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(100);
                
            usageEntity.Property(e => e.Environment)
                .IsRequired()
                .HasMaxLength(50);
                
            usageEntity.Property(e => e.AdditionalData)
                .HasMaxLength(4000);
        }
    }
}

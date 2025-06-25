using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ToggleNet.EntityFrameworkCore.Migrations.SqlServer
{
    public partial class InitialMigrationSqlServer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ToggleNet_FeatureFlags",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false),
                    RolloutPercentage = table.Column<int>(nullable: false, defaultValue: 0),
                    Environment = table.Column<string>(maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToggleNet_FeatureFlags", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToggleNet_FeatureFlags_Name",
                table: "ToggleNet_FeatureFlags",
                column: "Name",
                unique: true);
                
            // Add some example feature flags for testing
            migrationBuilder.InsertData(
                table: "ToggleNet_FeatureFlags",
                columns: new[] { "Id", "Name", "Description", "IsEnabled", "RolloutPercentage", "Environment", "UpdatedAt" },
                values: new object[] { Guid.NewGuid(), "new-dashboard", "Enable the new dashboard UI", true, 50, "Development", DateTime.UtcNow });
            
            migrationBuilder.InsertData(
                table: "ToggleNet_FeatureFlags",
                columns: new[] { "Id", "Name", "Description", "IsEnabled", "RolloutPercentage", "Environment", "UpdatedAt" },
                values: new object[] { Guid.NewGuid(), "beta-features", "Enable beta features for testing", true, 25, "Development", DateTime.UtcNow });
                
            migrationBuilder.InsertData(
                table: "ToggleNet_FeatureFlags",
                columns: new[] { "Id", "Name", "Description", "IsEnabled", "RolloutPercentage", "Environment", "UpdatedAt" },
                values: new object[] { Guid.NewGuid(), "dark-mode", "Enable dark mode UI", true, 100, "Development", DateTime.UtcNow });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToggleNet_FeatureFlags");
        }
    }
}

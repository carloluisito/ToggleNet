using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ToggleNet.EntityFrameworkCore.Migrations.SqlServer
{
    public partial class AddFeatureUsagesSqlServer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ToggleNet_FeatureUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FeatureName = table.Column<string>(maxLength: 100, nullable: false),
                    UserId = table.Column<string>(maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Environment = table.Column<string>(maxLength: 50, nullable: false),
                    AdditionalData = table.Column<string>(maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToggleNet_FeatureUsages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToggleNet_FeatureUsages_FeatureName",
                table: "ToggleNet_FeatureUsages",
                column: "FeatureName");
                
            migrationBuilder.CreateIndex(
                name: "IX_ToggleNet_FeatureUsages_UserId",
                table: "ToggleNet_FeatureUsages",
                column: "UserId");
                
            migrationBuilder.CreateIndex(
                name: "IX_ToggleNet_FeatureUsages_Environment",
                table: "ToggleNet_FeatureUsages",
                column: "Environment");
                
            migrationBuilder.CreateIndex(
                name: "IX_ToggleNet_FeatureUsages_Timestamp",
                table: "ToggleNet_FeatureUsages",
                column: "Timestamp");
                
            migrationBuilder.CreateIndex(
                name: "IX_ToggleNet_FeatureUsages_FeatureName_Environment",
                table: "ToggleNet_FeatureUsages",
                columns: new[] { "FeatureName", "Environment" });
                
            migrationBuilder.CreateIndex(
                name: "IX_ToggleNet_FeatureUsages_FeatureName_UserId_Environment",
                table: "ToggleNet_FeatureUsages",
                columns: new[] { "FeatureName", "UserId", "Environment" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToggleNet_FeatureUsages");
        }
    }
}

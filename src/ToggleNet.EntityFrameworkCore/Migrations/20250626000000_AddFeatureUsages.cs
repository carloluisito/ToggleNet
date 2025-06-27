using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ToggleNet.EntityFrameworkCore.Migrations
{
    public partial class AddFeatureUsages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeatureUsages",
                schema: "togglenet",
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
                    table.PrimaryKey("PK_FeatureUsages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureUsages_FeatureName",
                schema: "togglenet",
                table: "FeatureUsages",
                column: "FeatureName");
                
            migrationBuilder.CreateIndex(
                name: "IX_FeatureUsages_UserId",
                schema: "togglenet",
                table: "FeatureUsages",
                column: "UserId");
                
            migrationBuilder.CreateIndex(
                name: "IX_FeatureUsages_Environment",
                schema: "togglenet",
                table: "FeatureUsages",
                column: "Environment");
                
            migrationBuilder.CreateIndex(
                name: "IX_FeatureUsages_Timestamp",
                schema: "togglenet",
                table: "FeatureUsages",
                column: "Timestamp");
                
            migrationBuilder.CreateIndex(
                name: "IX_FeatureUsages_FeatureName_Environment",
                schema: "togglenet",
                table: "FeatureUsages",
                columns: new[] { "FeatureName", "Environment" });
                
            migrationBuilder.CreateIndex(
                name: "IX_FeatureUsages_FeatureName_UserId_Environment",
                schema: "togglenet",
                table: "FeatureUsages",
                columns: new[] { "FeatureName", "UserId", "Environment" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureUsages",
                schema: "togglenet");
        }
    }
}

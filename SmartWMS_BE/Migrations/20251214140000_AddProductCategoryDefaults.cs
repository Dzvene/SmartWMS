using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCategoryDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Product Defaults
            migrationBuilder.AddColumn<string>(
                name: "DefaultUnitOfMeasure",
                table: "ProductCategories",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultStorageZoneType",
                table: "ProductCategories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // Tracking Requirements
            migrationBuilder.AddColumn<bool>(
                name: "RequiresBatchTracking",
                table: "ProductCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresSerialTracking",
                table: "ProductCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresExpiryDate",
                table: "ProductCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Handling & Storage
            migrationBuilder.AddColumn<string>(
                name: "HandlingInstructions",
                table: "ProductCategories",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinTemperature",
                table: "ProductCategories",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxTemperature",
                table: "ProductCategories",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHazardous",
                table: "ProductCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFragile",
                table: "ProductCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultUnitOfMeasure",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DefaultStorageZoneType",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "RequiresBatchTracking",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "RequiresSerialTracking",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "RequiresExpiryDate",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "HandlingInstructions",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "MinTemperature",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "MaxTemperature",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "IsHazardous",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "IsFragile",
                table: "ProductCategories");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Byd.CoreExtensions.Migrations.Settings
{
    /// <inheritdoc />
    public partial class RenameAbpColumnsToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                schema: "abp",
                table: "SettingDefinitions",
                newName: "extra_properties");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "extra_properties",
                schema: "abp",
                table: "SettingDefinitions",
                newName: "ExtraProperties");
        }
    }
}

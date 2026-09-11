using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Byd.CoreExtensions.Migrations.Settings
{
    /// <inheritdoc />
    public partial class CreateSettingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "abp");

            migrationBuilder.CreateTable(
                name: "SettingDefinitions",
                schema: "abp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    default_value = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    is_visible_to_clients = table.Column<bool>(type: "boolean", nullable: false),
                    providers = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    is_inherited = table.Column<bool>(type: "boolean", nullable: false),
                    is_encrypted = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_setting_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                schema: "abp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    value = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    provider_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    provider_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_settings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_setting_definitions_name",
                schema: "abp",
                table: "SettingDefinitions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_settings_name_provider_name_provider_key",
                schema: "abp",
                table: "Settings",
                columns: new[] { "name", "provider_name", "provider_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SettingDefinitions",
                schema: "abp");

            migrationBuilder.DropTable(
                name: "Settings",
                schema: "abp");
        }
    }
}

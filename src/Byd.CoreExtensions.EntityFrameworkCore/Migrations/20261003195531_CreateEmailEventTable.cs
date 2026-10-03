using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Byd.CoreExtensions.Migrations
{
    /// <inheritdoc />
    public partial class CreateEmailEventTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eski satirlar olaya enum degeriyle bagliydi ve yeni olay kayitlarina eslenemez; zorunlu FK kolonu eklenmeden
            // once silinir. Varsayilan sablonlari olayi gonderen modulun seed'i yeniden acar.
            migrationBuilder.Sql("DELETE FROM core_extension.\"CoreExtensionsEmailTemplates\";");

            migrationBuilder.DropIndex(
                name: "IX_EmailTemplates_Event_CultureName_TenantId_OrganizationId",
                schema: "core_extension",
                table: "CoreExtensionsEmailTemplates");

            migrationBuilder.DropColumn(
                name: "event",
                schema: "core_extension",
                table: "CoreExtensionsEmailTemplates");

            migrationBuilder.AddColumn<Guid>(
                name: "email_event_id",
                schema: "core_extension",
                table: "CoreExtensionsEmailTemplates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "CoreExtensionsEmailEvents",
                schema: "core_extension",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    code = table.Column<int>(type: "integer", nullable: true),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_core_extensions_email_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_EmailEventId_CultureName_TenantId_OrganizationId",
                schema: "core_extension",
                table: "CoreExtensionsEmailTemplates",
                columns: new[] { "email_event_id", "culture_name", "tenant_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailEvents_Name",
                schema: "core_extension",
                table: "CoreExtensionsEmailEvents",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_core_extensions_email_templates_core_extensions_emai",
                schema: "core_extension",
                table: "CoreExtensionsEmailTemplates",
                column: "email_event_id",
                principalSchema: "core_extension",
                principalTable: "CoreExtensionsEmailEvents",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_core_extensions_email_templates_core_extensions_emai",
                schema: "core_extension",
                table: "CoreExtensionsEmailTemplates");

            migrationBuilder.DropTable(
                name: "CoreExtensionsEmailEvents",
                schema: "core_extension");

            migrationBuilder.DropIndex(
                name: "IX_EmailTemplates_EmailEventId_CultureName_TenantId_OrganizationId",
                schema: "core_extension",
                table: "CoreExtensionsEmailTemplates");

            migrationBuilder.DropColumn(
                name: "email_event_id",
                schema: "core_extension",
                table: "CoreExtensionsEmailTemplates");

            migrationBuilder.AddColumn<int>(
                name: "event",
                schema: "core_extension",
                table: "CoreExtensionsEmailTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Event_CultureName_TenantId_OrganizationId",
                schema: "core_extension",
                table: "CoreExtensionsEmailTemplates",
                columns: new[] { "event", "culture_name", "tenant_id", "organization_id" });
        }
    }
}

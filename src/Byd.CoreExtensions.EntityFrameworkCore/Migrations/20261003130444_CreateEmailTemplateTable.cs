using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Byd.CoreExtensions.Migrations
{
    /// <inheritdoc />
    public partial class CreateEmailTemplateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoreExtensionsEmailTemplates",
                schema: "core_extension",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    @event = table.Column<int>(name: "event", type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    culture_name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    extra_properties = table.Column<string>(type: "text", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_core_extensions_email_templates", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Event_CultureName_TenantId_OrganizationId",
                schema: "core_extension",
                table: "CoreExtensionsEmailTemplates",
                columns: new[] { "event", "culture_name", "tenant_id", "organization_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoreExtensionsEmailTemplates",
                schema: "core_extension");
        }
    }
}

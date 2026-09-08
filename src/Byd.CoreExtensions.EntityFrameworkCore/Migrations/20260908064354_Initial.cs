using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Byd.CoreExtensions.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core_extension");

            migrationBuilder.CreateTable(
                name: "EventInbox",
                schema: "core_extension",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    message_id = table.Column<string>(type: "text", nullable: false),
                    event_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    event_data = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    handled_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    next_retry_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    owner_environment_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    owner_module_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_inbox", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "EventOutbox",
                schema: "core_extension",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    event_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    event_data = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_outbox", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "FailedEventInboxes",
                schema: "core_extension",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    event_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    event_data = table.Column<byte[]>(type: "bytea", nullable: false),
                    owner_module_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    owner_environment_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    processed_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    not_before = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    exception_message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    exception_stack_trace = table.Column<string>(type: "text", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_failed_event_inboxes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CoreExtensionsApiKeys",
                schema: "core_extension",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_core_extensions_api_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CoreExtensionsModuleSettings",
                schema: "core_extension",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    ttl_seconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    rate_limit_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    rate_limit_permit_limit = table.Column<int>(type: "integer", nullable: false, defaultValue: 300),
                    rate_limit_window_seconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    idempotency_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    idempotency_ttl_seconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 300),
                    event_box_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    failed_event_retry_unlimited = table.Column<bool>(type: "boolean", nullable: false),
                    failed_event_retry_window_seconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 86400),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_core_extensions_module_settings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_event_inbox_message_id",
                schema: "core_extension",
                table: "EventInbox",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_inbox_status_creation_time",
                schema: "core_extension",
                table: "EventInbox",
                columns: new[] { "status", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "ix_event_outbox_creation_time",
                schema: "core_extension",
                table: "EventOutbox",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_FailedEventInboxes_MessageId",
                schema: "core_extension",
                table: "FailedEventInboxes",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "IX_FailedEventInboxes_Scope_Retry",
                schema: "core_extension",
                table: "FailedEventInboxes",
                columns: new[] { "owner_module_name", "owner_environment_name", "is_processed", "not_before" });

            migrationBuilder.CreateIndex(
                name: "ix_core_extensions_api_keys_key",
                schema: "core_extension",
                table: "CoreExtensionsApiKeys",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleSettings_ModuleName_TenantId",
                schema: "core_extension",
                table: "CoreExtensionsModuleSettings",
                columns: new[] { "module_name", "tenant_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventInbox",
                schema: "core_extension");

            migrationBuilder.DropTable(
                name: "EventOutbox",
                schema: "core_extension");

            migrationBuilder.DropTable(
                name: "FailedEventInboxes",
                schema: "core_extension");

            migrationBuilder.DropTable(
                name: "CoreExtensionsApiKeys",
                schema: "core_extension");

            migrationBuilder.DropTable(
                name: "CoreExtensionsModuleSettings",
                schema: "core_extension");
        }
    }
}

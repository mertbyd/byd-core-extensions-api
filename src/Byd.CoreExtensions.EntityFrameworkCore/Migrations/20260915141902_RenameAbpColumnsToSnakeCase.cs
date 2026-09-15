using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Byd.CoreExtensions.Migrations
{
    /// <inheritdoc />
    public partial class RenameAbpColumnsToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastModifierId",
                schema: "core_extension",
                table: "CoreExtensionsModuleSettings",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "LastModificationTime",
                schema: "core_extension",
                table: "CoreExtensionsModuleSettings",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                schema: "core_extension",
                table: "CoreExtensionsModuleSettings",
                newName: "extra_properties");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                schema: "core_extension",
                table: "CoreExtensionsModuleSettings",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                schema: "core_extension",
                table: "CoreExtensionsModuleSettings",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                schema: "core_extension",
                table: "CoreExtensionsModuleSettings",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                schema: "core_extension",
                table: "CoreExtensionsApiKeys",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                schema: "core_extension",
                table: "CoreExtensionsApiKeys",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "LastModifierId",
                schema: "core_extension",
                table: "FailedEventInboxes",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "LastModificationTime",
                schema: "core_extension",
                table: "FailedEventInboxes",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                schema: "core_extension",
                table: "FailedEventInboxes",
                newName: "extra_properties");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                schema: "core_extension",
                table: "FailedEventInboxes",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                schema: "core_extension",
                table: "FailedEventInboxes",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                schema: "core_extension",
                table: "FailedEventInboxes",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                schema: "core_extension",
                table: "EventOutbox",
                newName: "extra_properties");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                schema: "core_extension",
                table: "EventOutbox",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                schema: "core_extension",
                table: "EventInbox",
                newName: "extra_properties");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                schema: "core_extension",
                table: "EventInbox",
                newName: "creation_time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                schema: "core_extension",
                table: "CoreExtensionsModuleSettings",
                newName: "LastModifierId");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                schema: "core_extension",
                table: "CoreExtensionsModuleSettings",
                newName: "LastModificationTime");

            migrationBuilder.RenameColumn(
                name: "extra_properties",
                schema: "core_extension",
                table: "CoreExtensionsModuleSettings",
                newName: "ExtraProperties");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                schema: "core_extension",
                table: "CoreExtensionsModuleSettings",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                schema: "core_extension",
                table: "CoreExtensionsModuleSettings",
                newName: "CreationTime");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                schema: "core_extension",
                table: "CoreExtensionsModuleSettings",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                schema: "core_extension",
                table: "CoreExtensionsApiKeys",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                schema: "core_extension",
                table: "CoreExtensionsApiKeys",
                newName: "CreationTime");

            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                schema: "core_extension",
                table: "FailedEventInboxes",
                newName: "LastModifierId");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                schema: "core_extension",
                table: "FailedEventInboxes",
                newName: "LastModificationTime");

            migrationBuilder.RenameColumn(
                name: "extra_properties",
                schema: "core_extension",
                table: "FailedEventInboxes",
                newName: "ExtraProperties");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                schema: "core_extension",
                table: "FailedEventInboxes",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                schema: "core_extension",
                table: "FailedEventInboxes",
                newName: "CreationTime");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                schema: "core_extension",
                table: "FailedEventInboxes",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "extra_properties",
                schema: "core_extension",
                table: "EventOutbox",
                newName: "ExtraProperties");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                schema: "core_extension",
                table: "EventOutbox",
                newName: "CreationTime");

            migrationBuilder.RenameColumn(
                name: "extra_properties",
                schema: "core_extension",
                table: "EventInbox",
                newName: "ExtraProperties");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                schema: "core_extension",
                table: "EventInbox",
                newName: "CreationTime");
        }
    }
}

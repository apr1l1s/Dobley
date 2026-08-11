using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dobley.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSoftDeleteUniqueFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StorageNotificationSubscriptions_NotificationRecipientId_St~",
                table: "StorageNotificationSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_NotificationRecipients_UserName_Channel_ExternalId",
                table: "NotificationRecipients");

            migrationBuilder.CreateIndex(
                name: "IX_StorageNotificationSubscriptions_NotificationRecipientId_St~",
                table: "StorageNotificationSubscriptions",
                columns: new[] { "NotificationRecipientId", "StorageId" },
                unique: true,
                filter: "\"DateDeleted\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_UserName_Channel_ExternalId",
                table: "NotificationRecipients",
                columns: new[] { "UserName", "Channel", "ExternalId" },
                unique: true,
                filter: "\"DateDeleted\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StorageNotificationSubscriptions_NotificationRecipientId_St~",
                table: "StorageNotificationSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_NotificationRecipients_UserName_Channel_ExternalId",
                table: "NotificationRecipients");

            migrationBuilder.CreateIndex(
                name: "IX_StorageNotificationSubscriptions_NotificationRecipientId_St~",
                table: "StorageNotificationSubscriptions",
                columns: new[] { "NotificationRecipientId", "StorageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_UserName_Channel_ExternalId",
                table: "NotificationRecipients",
                columns: new[] { "UserName", "Channel", "ExternalId" },
                unique: true);
        }
    }
}

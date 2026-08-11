using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dobley.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class AllowSharedNotificationRecipients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationRecipients_Channel_ExternalId",
                table: "NotificationRecipients");

            migrationBuilder.DropIndex(
                name: "IX_NotificationRecipients_UserName",
                table: "NotificationRecipients");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_UserName_Channel_ExternalId",
                table: "NotificationRecipients",
                columns: new[] { "UserName", "Channel", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationRecipients_UserName_Channel_ExternalId",
                table: "NotificationRecipients");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_Channel_ExternalId",
                table: "NotificationRecipients",
                columns: new[] { "Channel", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_UserName",
                table: "NotificationRecipients",
                column: "UserName");
        }
    }
}

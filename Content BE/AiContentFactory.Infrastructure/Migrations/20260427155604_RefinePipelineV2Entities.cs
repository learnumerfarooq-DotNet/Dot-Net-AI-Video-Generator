using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiContentFactory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefinePipelineV2Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentKey = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FacebookCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentKey = table.Column<string>(type: "text", nullable: false),
                    AppId = table.Column<string>(type: "text", nullable: false),
                    AppSecret = table.Column<string>(type: "text", nullable: false),
                    PageAccessToken = table.Column<string>(type: "text", nullable: false),
                    PageId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacebookCredentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstagramCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentKey = table.Column<string>(type: "text", nullable: false),
                    FacebookAppId = table.Column<string>(type: "text", nullable: false),
                    FacebookAppSecret = table.Column<string>(type: "text", nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    InstagramAccountId = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstagramCredentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LinkedInCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentKey = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    ClientSecret = table.Column<string>(type: "text", nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkedInCredentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TikTokCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentKey = table.Column<string>(type: "text", nullable: false),
                    ClientKey = table.Column<string>(type: "text", nullable: false),
                    ClientSecret = table.Column<string>(type: "text", nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TikTokCredentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YouTubeCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentKey = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    ClientSecret = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    TokenExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChannelId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouTubeCredentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YouTubeVideoDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    YouTubeVideoId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    CategoryId = table.Column<string>(type: "text", nullable: false),
                    Privacy = table.Column<string>(type: "text", nullable: false),
                    ScheduledPublishAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ThumbnailPath = table.Column<string>(type: "text", nullable: true),
                    IsShort = table.Column<bool>(type: "boolean", nullable: false),
                    PlaylistId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouTubeVideoDetails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YouTubeUploadResults_YouTubeVideoId",
                table: "YouTubeUploadResults",
                column: "YouTubeVideoId");

            migrationBuilder.CreateIndex(
                name: "IX_TikTokUploadResults_TikTokVideoId",
                table: "TikTokUploadResults",
                column: "TikTokVideoId");

            migrationBuilder.CreateIndex(
                name: "IX_InstagramUploadResults_InstagramMediaId",
                table: "InstagramUploadResults",
                column: "InstagramMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacebookCredentials_AgentKey",
                table: "FacebookCredentials",
                column: "AgentKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstagramCredentials_AgentKey",
                table: "InstagramCredentials",
                column: "AgentKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LinkedInCredentials_AgentKey",
                table: "LinkedInCredentials",
                column: "AgentKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TikTokCredentials_AgentKey",
                table: "TikTokCredentials",
                column: "AgentKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YouTubeCredentials_AgentKey",
                table: "YouTubeCredentials",
                column: "AgentKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorLogs");

            migrationBuilder.DropTable(
                name: "FacebookCredentials");

            migrationBuilder.DropTable(
                name: "InstagramCredentials");

            migrationBuilder.DropTable(
                name: "LinkedInCredentials");

            migrationBuilder.DropTable(
                name: "TikTokCredentials");

            migrationBuilder.DropTable(
                name: "YouTubeCredentials");

            migrationBuilder.DropTable(
                name: "YouTubeVideoDetails");

            migrationBuilder.DropIndex(
                name: "IX_YouTubeUploadResults_YouTubeVideoId",
                table: "YouTubeUploadResults");

            migrationBuilder.DropIndex(
                name: "IX_TikTokUploadResults_TikTokVideoId",
                table: "TikTokUploadResults");

            migrationBuilder.DropIndex(
                name: "IX_InstagramUploadResults_InstagramMediaId",
                table: "InstagramUploadResults");
        }
    }
}

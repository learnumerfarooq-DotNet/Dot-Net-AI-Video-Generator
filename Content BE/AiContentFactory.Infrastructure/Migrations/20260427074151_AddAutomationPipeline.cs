using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiContentFactory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomationPipeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentDecisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentKey = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Outcome = table.Column<string>(type: "text", nullable: false),
                    RawJsonPayload = table.Column<string>(type: "text", nullable: false),
                    ValidatedPayload = table.Column<string>(type: "text", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "double precision", nullable: false),
                    PromptVersion = table.Column<string>(type: "text", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_AgentDecisions", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "DecisionCacheEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CacheKey = table.Column<string>(type: "text", nullable: false),
                    JsonPayload = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_DecisionCacheEntries", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "DecisionValidations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DecisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ValidatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_DecisionValidations", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "ErrorQueue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: false),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    IsPermanent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_ErrorQueue", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "PromptTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentKey = table.Column<string>(type: "text", nullable: false),
                    DecisionType = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    SystemPrompt = table.Column<string>(type: "text", nullable: false),
                    UserPromptTemplate = table.Column<string>(type: "text", nullable: false),
                    JsonOutputSchema = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_PromptTemplates", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "VideoPipelineJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriveFileId = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CurrentStage = table.Column<string>(type: "text", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_VideoPipelineJobs", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "PipelineStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineStages_VideoPipelineJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "VideoPipelineJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Add columns to existing tables
            migrationBuilder.AddColumn<Guid>(
                name: "SystemPromptTemplateId",
                table: "studio_agents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionOutputSchema",
                table: "studio_agents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DecisionId",
                table: "studio_chat_messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStructuredOutput",
                table: "studio_chat_messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_DecisionCacheEntries_CacheKey",
                table: "DecisionCacheEntries",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PipelineStages_JobId",
                table: "PipelineStages",
                column: "JobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AgentDecisions");
            migrationBuilder.DropTable(name: "DecisionCacheEntries");
            migrationBuilder.DropTable(name: "DecisionValidations");
            migrationBuilder.DropTable(name: "ErrorQueue");
            migrationBuilder.DropTable(name: "PipelineStages");
            migrationBuilder.DropTable(name: "PromptTemplates");
            migrationBuilder.DropTable(name: "VideoPipelineJobs");

            migrationBuilder.DropColumn(name: "SystemPromptTemplateId", table: "studio_agents");
            migrationBuilder.DropColumn(name: "DecisionOutputSchema", table: "studio_agents");
            migrationBuilder.DropColumn(name: "DecisionId", table: "studio_chat_messages");
            migrationBuilder.DropColumn(name: "IsStructuredOutput", table: "studio_chat_messages");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiContentFactory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ManualFixMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL to add columns safely (if they don't exist)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='studio_chat_messages' AND column_name='DecisionId') THEN
                        ALTER TABLE studio_chat_messages ADD COLUMN ""DecisionId"" uuid;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='studio_chat_messages' AND column_name='IsStructuredOutput') THEN
                        ALTER TABLE studio_chat_messages ADD COLUMN ""IsStructuredOutput"" boolean DEFAULT false;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='studio_agents' AND column_name='SystemPromptTemplateId') THEN
                        ALTER TABLE studio_agents ADD COLUMN ""SystemPromptTemplateId"" uuid;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='studio_agents' AND column_name='DecisionOutputSchema') THEN
                        ALTER TABLE studio_agents ADD COLUMN ""DecisionOutputSchema"" text;
                    END IF;
                END
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop columns safely
            migrationBuilder.Sql(@"
                ALTER TABLE studio_chat_messages DROP COLUMN IF EXISTS ""DecisionId"";
                ALTER TABLE studio_chat_messages DROP COLUMN IF EXISTS ""IsStructuredOutput"";
                ALTER TABLE studio_agents DROP COLUMN IF EXISTS ""SystemPromptTemplateId"";
                ALTER TABLE studio_agents DROP COLUMN IF EXISTS ""DecisionOutputSchema"";
            ");
        }
    }
}

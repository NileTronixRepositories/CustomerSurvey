using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TemplateQuestionCondition_ParentTemplateQuestionId_ChildTemplateQuestionId_TriggerType_SelectedQuestionOptionId_TriggerValue",
                table: "TemplateQuestionCondition");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateQuestionCondition_ParentTemplateQuestionId",
                table: "TemplateQuestionCondition",
                column: "ParentTemplateQuestionId");

            migrationBuilder.CreateIndex(
                name: "UX_TQC_SingleChoice",
                table: "TemplateQuestionCondition",
                columns: new[] { "TemplateId", "ParentTemplateQuestionId", "ChildTemplateQuestionId", "TriggerType", "SelectedQuestionOptionId" },
                unique: true,
                filter: "[TriggerType] = 1 AND [SelectedQuestionOptionId] IS NOT NULL AND [TriggerValue] IS NULL AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_TQC_ValueTrigger",
                table: "TemplateQuestionCondition",
                columns: new[] { "TemplateId", "ParentTemplateQuestionId", "ChildTemplateQuestionId", "TriggerType", "TriggerValue" },
                unique: true,
                filter: "[TriggerType] IN (2, 3) AND [SelectedQuestionOptionId] IS NULL AND [TriggerValue] IS NOT NULL AND [IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TemplateQuestionCondition_ParentTemplateQuestionId",
                table: "TemplateQuestionCondition");

            migrationBuilder.DropIndex(
                name: "UX_TQC_SingleChoice",
                table: "TemplateQuestionCondition");

            migrationBuilder.DropIndex(
                name: "UX_TQC_ValueTrigger",
                table: "TemplateQuestionCondition");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateQuestionCondition_ParentTemplateQuestionId_ChildTemplateQuestionId_TriggerType_SelectedQuestionOptionId_TriggerValue",
                table: "TemplateQuestionCondition",
                columns: new[] { "ParentTemplateQuestionId", "ChildTemplateQuestionId", "TriggerType", "SelectedQuestionOptionId", "TriggerValue" },
                unique: true,
                filter: "[SelectedQuestionOptionId] IS NOT NULL AND [TriggerValue] IS NOT NULL");
        }
    }
}

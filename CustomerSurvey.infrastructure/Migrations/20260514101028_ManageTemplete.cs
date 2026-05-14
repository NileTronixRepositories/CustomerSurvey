using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ManageTemplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TemplateQuestionCondition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentTemplateQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChildTemplateQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TriggerType = table.Column<int>(type: "int", nullable: false),
                    SelectedQuestionOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TriggerValue = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateQuestionCondition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateQuestionCondition_QuestionOption_SelectedQuestionOptionId",
                        column: x => x.SelectedQuestionOptionId,
                        principalTable: "QuestionOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemplateQuestionCondition_TemplateQuestion_ChildTemplateQuestionId",
                        column: x => x.ChildTemplateQuestionId,
                        principalTable: "TemplateQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemplateQuestionCondition_TemplateQuestion_ParentTemplateQuestionId",
                        column: x => x.ParentTemplateQuestionId,
                        principalTable: "TemplateQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemplateQuestionCondition_Template_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Template",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateQuestionCondition_ChildTemplateQuestionId",
                table: "TemplateQuestionCondition",
                column: "ChildTemplateQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateQuestionCondition_ParentTemplateQuestionId_ChildTemplateQuestionId_TriggerType_SelectedQuestionOptionId_TriggerValue",
                table: "TemplateQuestionCondition",
                columns: new[] { "ParentTemplateQuestionId", "ChildTemplateQuestionId", "TriggerType", "SelectedQuestionOptionId", "TriggerValue" },
                unique: true,
                filter: "[SelectedQuestionOptionId] IS NOT NULL AND [TriggerValue] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateQuestionCondition_SelectedQuestionOptionId",
                table: "TemplateQuestionCondition",
                column: "SelectedQuestionOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateQuestionCondition_TemplateId",
                table: "TemplateQuestionCondition",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateQuestionCondition_TemplateId_ChildTemplateQuestionId",
                table: "TemplateQuestionCondition",
                columns: new[] { "TemplateId", "ChildTemplateQuestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateQuestionCondition_TemplateId_ParentTemplateQuestionId",
                table: "TemplateQuestionCondition",
                columns: new[] { "TemplateId", "ParentTemplateQuestionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateQuestionCondition");
        }
    }
}

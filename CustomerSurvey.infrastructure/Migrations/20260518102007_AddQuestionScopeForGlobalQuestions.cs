using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionScopeForGlobalQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestionGroup_BranchId_NameEn",
                table: "QuestionGroup");

            migrationBuilder.AlterColumn<Guid>(
                name: "BranchId",
                table: "QuestionGroup",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "QuestionGroup",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<Guid>(
                name: "BranchId",
                table: "Question",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "Question",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionGroup_Scope_IsActive",
                table: "QuestionGroup",
                columns: new[] { "Scope", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "UX_QuestionGroup_Branch_NameEn",
                table: "QuestionGroup",
                columns: new[] { "BranchId", "NameEn" },
                unique: true,
                filter: "[Scope] = 1 AND [BranchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_QuestionGroup_Global_NameEn",
                table: "QuestionGroup",
                column: "NameEn",
                unique: true,
                filter: "[Scope] = 2 AND [BranchId] IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuestionGroup_Scope_BranchId",
                table: "QuestionGroup",
                sql: "([Scope] = 1 AND [BranchId] IS NOT NULL) OR ([Scope] = 2 AND [BranchId] IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Question_BranchId_IsActive",
                table: "Question",
                columns: new[] { "BranchId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Question_Scope",
                table: "Question",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_Question_Scope_IsActive",
                table: "Question",
                columns: new[] { "Scope", "IsActive" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Question_Scope_BranchId",
                table: "Question",
                sql: "([Scope] = 1 AND [BranchId] IS NOT NULL) OR ([Scope] = 2 AND [BranchId] IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestionGroup_Scope_IsActive",
                table: "QuestionGroup");

            migrationBuilder.DropIndex(
                name: "UX_QuestionGroup_Branch_NameEn",
                table: "QuestionGroup");

            migrationBuilder.DropIndex(
                name: "UX_QuestionGroup_Global_NameEn",
                table: "QuestionGroup");

            migrationBuilder.DropCheckConstraint(
                name: "CK_QuestionGroup_Scope_BranchId",
                table: "QuestionGroup");

            migrationBuilder.DropIndex(
                name: "IX_Question_BranchId_IsActive",
                table: "Question");

            migrationBuilder.DropIndex(
                name: "IX_Question_Scope",
                table: "Question");

            migrationBuilder.DropIndex(
                name: "IX_Question_Scope_IsActive",
                table: "Question");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Question_Scope_BranchId",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "QuestionGroup");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Question");

            migrationBuilder.AlterColumn<Guid>(
                name: "BranchId",
                table: "QuestionGroup",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BranchId",
                table: "Question",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionGroup_BranchId_NameEn",
                table: "QuestionGroup",
                columns: new[] { "BranchId", "NameEn" },
                unique: true);
        }
    }
}

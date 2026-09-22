using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TemplateSystemEnhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Template");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AnonymousTemplate");

            migrationBuilder.AddColumn<string>(
                name: "LogoPath",
                table: "Template",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginTemplateId",
                table: "Template",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateFamilyId",
                table: "Template",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginQuestionOptionId",
                table: "QuestionOption",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginQuestionGroupId",
                table: "QuestionGroup",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginQuestionId",
                table: "Question",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PublicUrl",
                table: "AnonymousTemplate",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "AnonymousTemplate",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LogoPath",
                table: "AnonymousTemplate",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginAnonymousTemplateId",
                table: "AnonymousTemplate",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceGlobalAnonymousTemplateId",
                table: "AnonymousTemplate",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateFamilyId",
                table: "AnonymousTemplate",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE [AnonymousTemplate] SET [IsActive] = 0, [IsArchived] = 0, [PublicUrl] = NULL, [QrCode] = NULL, [LogoPath] = NULL WHERE [Scope] = 2;");

            migrationBuilder.CreateIndex(
                name: "IX_Template_BranchId_TemplateFamilyId",
                table: "Template",
                columns: new[] { "BranchId", "TemplateFamilyId" });

            migrationBuilder.CreateIndex(
                name: "UX_Template_Branch_OriginTemplateId",
                table: "Template",
                columns: new[] { "BranchId", "OriginTemplateId" },
                unique: true,
                filter: "[OriginTemplateId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_QuestionOption_Question_OriginQuestionOptionId",
                table: "QuestionOption",
                columns: new[] { "QuestionId", "OriginQuestionOptionId" },
                unique: true,
                filter: "[OriginQuestionOptionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_QuestionGroup_Branch_OriginQuestionGroupId",
                table: "QuestionGroup",
                columns: new[] { "BranchId", "OriginQuestionGroupId" },
                unique: true,
                filter: "[OriginQuestionGroupId] IS NOT NULL AND [BranchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Question_Branch_OriginQuestionId",
                table: "Question",
                columns: new[] { "BranchId", "OriginQuestionId" },
                unique: true,
                filter: "[OriginQuestionId] IS NOT NULL AND [BranchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplate_BranchId_TemplateFamilyId",
                table: "AnonymousTemplate",
                columns: new[] { "BranchId", "TemplateFamilyId" });

            migrationBuilder.CreateIndex(
                name: "UX_AnonymousTemplate_Branch_GlobalSource",
                table: "AnonymousTemplate",
                columns: new[] { "BranchId", "SourceGlobalAnonymousTemplateId" },
                unique: true,
                filter: "[SourceGlobalAnonymousTemplateId] IS NOT NULL AND [BranchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_AnonymousTemplate_Branch_OriginAnonymousTemplateId",
                table: "AnonymousTemplate",
                columns: new[] { "BranchId", "OriginAnonymousTemplateId" },
                unique: true,
                filter: "[OriginAnonymousTemplateId] IS NOT NULL AND [BranchId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Template_BranchId_TemplateFamilyId",
                table: "Template");

            migrationBuilder.DropIndex(
                name: "UX_Template_Branch_OriginTemplateId",
                table: "Template");

            migrationBuilder.DropIndex(
                name: "UX_QuestionOption_Question_OriginQuestionOptionId",
                table: "QuestionOption");

            migrationBuilder.DropIndex(
                name: "UX_QuestionGroup_Branch_OriginQuestionGroupId",
                table: "QuestionGroup");

            migrationBuilder.DropIndex(
                name: "UX_Question_Branch_OriginQuestionId",
                table: "Question");

            migrationBuilder.DropIndex(
                name: "IX_AnonymousTemplate_BranchId_TemplateFamilyId",
                table: "AnonymousTemplate");

            migrationBuilder.DropIndex(
                name: "UX_AnonymousTemplate_Branch_GlobalSource",
                table: "AnonymousTemplate");

            migrationBuilder.DropIndex(
                name: "UX_AnonymousTemplate_Branch_OriginAnonymousTemplateId",
                table: "AnonymousTemplate");

            migrationBuilder.DropColumn(
                name: "LogoPath",
                table: "Template");

            migrationBuilder.DropColumn(
                name: "OriginTemplateId",
                table: "Template");

            migrationBuilder.DropColumn(
                name: "TemplateFamilyId",
                table: "Template");

            migrationBuilder.DropColumn(
                name: "OriginQuestionOptionId",
                table: "QuestionOption");

            migrationBuilder.DropColumn(
                name: "OriginQuestionGroupId",
                table: "QuestionGroup");

            migrationBuilder.DropColumn(
                name: "OriginQuestionId",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "AnonymousTemplate");

            migrationBuilder.DropColumn(
                name: "LogoPath",
                table: "AnonymousTemplate");

            migrationBuilder.DropColumn(
                name: "OriginAnonymousTemplateId",
                table: "AnonymousTemplate");

            migrationBuilder.DropColumn(
                name: "SourceGlobalAnonymousTemplateId",
                table: "AnonymousTemplate");

            migrationBuilder.DropColumn(
                name: "TemplateFamilyId",
                table: "AnonymousTemplate");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Template",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE [AnonymousTemplate] SET [PublicUrl] = '' WHERE [PublicUrl] IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "PublicUrl",
                table: "AnonymousTemplate",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AnonymousTemplate",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

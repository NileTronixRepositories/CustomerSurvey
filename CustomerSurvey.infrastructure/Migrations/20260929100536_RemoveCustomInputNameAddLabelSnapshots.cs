using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCustomInputNameAddLabelSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LabelArSnapshot",
                table: "SurveyResponseCustomInputValue",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelEnSnapshot",
                table: "SurveyResponseCustomInputValue",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelArSnapshot",
                table: "AnonymousSurveyResponseCustomInputValue",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelEnSnapshot",
                table: "AnonymousSurveyResponseCustomInputValue",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [TemplateCustomInput]
                SET [LabelEn] = [Name]
                WHERE NULLIF(LTRIM(RTRIM([LabelEn])), '') IS NULL
                  AND NULLIF(LTRIM(RTRIM([LabelAr])), '') IS NULL;

                UPDATE [AnonymousTemplateCustomInput]
                SET [LabelEn] = [Name]
                WHERE NULLIF(LTRIM(RTRIM([LabelEn])), '') IS NULL
                  AND NULLIF(LTRIM(RTRIM([LabelAr])), '') IS NULL;

                UPDATE value
                SET [LabelEnSnapshot] = CASE
                        WHEN NULLIF(LTRIM(RTRIM(input.[LabelEn])), '') IS NOT NULL THEN input.[LabelEn]
                        WHEN NULLIF(LTRIM(RTRIM(input.[LabelAr])), '') IS NULL THEN value.[NameSnapshot]
                        ELSE NULL
                    END,
                    [LabelArSnapshot] = NULLIF(LTRIM(RTRIM(input.[LabelAr])), '')
                FROM [SurveyResponseCustomInputValue] value
                LEFT JOIN [TemplateCustomInput] input
                    ON input.[Id] = value.[TemplateCustomInputId];

                UPDATE value
                SET [LabelEnSnapshot] = CASE
                        WHEN NULLIF(LTRIM(RTRIM(input.[LabelEn])), '') IS NOT NULL THEN input.[LabelEn]
                        WHEN NULLIF(LTRIM(RTRIM(input.[LabelAr])), '') IS NULL THEN value.[NameSnapshot]
                        ELSE NULL
                    END,
                    [LabelArSnapshot] = NULLIF(LTRIM(RTRIM(input.[LabelAr])), '')
                FROM [AnonymousSurveyResponseCustomInputValue] value
                LEFT JOIN [AnonymousTemplateCustomInput] input
                    ON input.[Id] = value.[AnonymousTemplateCustomInputId];
                """);

            migrationBuilder.DropIndex(
                name: "IX_TemplateCustomInput_TemplateId_Name",
                table: "TemplateCustomInput");

            migrationBuilder.DropIndex(
                name: "IX_AnonymousTemplateCustomInput_AnonymousTemplateId_Name",
                table: "AnonymousTemplateCustomInput");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "TemplateCustomInput");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AnonymousTemplateCustomInput");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LabelArSnapshot",
                table: "SurveyResponseCustomInputValue");

            migrationBuilder.DropColumn(
                name: "LabelEnSnapshot",
                table: "SurveyResponseCustomInputValue");

            migrationBuilder.DropColumn(
                name: "LabelArSnapshot",
                table: "AnonymousSurveyResponseCustomInputValue");

            migrationBuilder.DropColumn(
                name: "LabelEnSnapshot",
                table: "AnonymousSurveyResponseCustomInputValue");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TemplateCustomInput",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AnonymousTemplateCustomInput",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE [TemplateCustomInput]
                SET [Name] = CONCAT('legacy_', CONVERT(nvarchar(36), [Id]));

                UPDATE [AnonymousTemplateCustomInput]
                SET [Name] = CONCAT('legacy_', CONVERT(nvarchar(36), [Id]));
                """);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCustomInput_TemplateId_Name",
                table: "TemplateCustomInput",
                columns: new[] { "TemplateId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateCustomInput_AnonymousTemplateId_Name",
                table: "AnonymousTemplateCustomInput",
                columns: new[] { "AnonymousTemplateId", "Name" },
                unique: true);
        }
    }
}

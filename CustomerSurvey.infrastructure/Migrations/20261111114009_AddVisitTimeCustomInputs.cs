using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitTimeCustomInputs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SystemFieldKey",
                table: "TemplateCustomInput",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateValue",
                table: "SurveyResponseCustomInputValue",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SystemFieldKey",
                table: "AnonymousTemplateCustomInput",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateValue",
                table: "AnonymousSurveyResponseCustomInputValue",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCustomInput_TemplateId_SystemFieldKey",
                table: "TemplateCustomInput",
                columns: new[] { "TemplateId", "SystemFieldKey" },
                unique: true,
                filter: "[SystemFieldKey] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateCustomInput_Date_Validation",
                table: "TemplateCustomInput",
                sql: "([Type] <> 3) OR ([MinLength] IS NULL AND [MaxLength] IS NULL AND [MinValue] IS NULL AND [MaxValue] IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseCustomInputValue_TemplateCustomInputId_DateValue_SurveyResponseId",
                table: "SurveyResponseCustomInputValue",
                columns: new[] { "TemplateCustomInputId", "DateValue", "SurveyResponseId" },
                filter: "[DateValue] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SRCIV_Date_Value",
                table: "SurveyResponseCustomInputValue",
                sql: "([TypeSnapshot] = 3 AND  [DateValue] IS NOT NULL AND [StringValue] IS NULL AND [IntegerValue] IS NULL) OR ([TypeSnapshot] <> 3 AND [DateValue] IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateCustomInput_AnonymousTemplateId_SystemFieldKey",
                table: "AnonymousTemplateCustomInput",
                columns: new[] { "AnonymousTemplateId", "SystemFieldKey" },
                unique: true,
                filter: "[SystemFieldKey] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AnonymousTemplateCustomInput_Date_Validation",
                table: "AnonymousTemplateCustomInput",
                sql: "([Type] <> 3) OR ([MinLength] IS NULL AND [MaxLength] IS NULL AND [MinValue] IS NULL AND [MaxValue] IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousSurveyResponseCustomInputValue_AnonymousTemplateCustomInputId_DateValue_AnonymousSurveyResponseId",
                table: "AnonymousSurveyResponseCustomInputValue",
                columns: new[] { "AnonymousTemplateCustomInputId", "DateValue", "AnonymousSurveyResponseId" },
                filter: "[DateValue] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ASRCIV_Date_Value",
                table: "AnonymousSurveyResponseCustomInputValue",
                sql: "([TypeSnapshot] = 3 AND  [DateValue] IS NOT NULL AND [StringValue] IS NULL AND [IntegerValue] IS NULL) OR ([TypeSnapshot] <> 3 AND [DateValue] IS NULL)");

            migrationBuilder.Sql("""
                INSERT INTO [TemplateCustomInput]
                    ([Id], [TemplateId], [LabelEn], [LabelAr], [Type], [IsRequired],
                     [Order], [IsActive], [CreatedByApplicationUserId], [CreatedOnUtc], [SystemFieldKey])
                SELECT NEWID(), t.[Id], N'Visit Time', N'تاريخ الزيارة', 3, 0,
                       CASE WHEN existing.[LastOrder] = 2147483647
                            THEN gap.[AvailableOrder] ELSE COALESCE(existing.[LastOrder], 0) + 1 END,
                       1, t.[CreatedByApplicationUserId], SYSUTCDATETIME(), 1
                FROM [Template] AS t
                OUTER APPLY (
                    SELECT MAX(CONVERT(bigint, i.[Order])) AS [LastOrder]
                    FROM [TemplateCustomInput] AS i
                    WHERE i.[TemplateId] = t.[Id]
                ) AS existing
                OUTER APPLY (
                    SELECT TOP (1) candidates.[AvailableOrder]
                    FROM (
                        SELECT CONVERT(bigint, 1) AS [AvailableOrder]
                        UNION ALL
                        SELECT CONVERT(bigint, i.[Order]) + 1
                        FROM [TemplateCustomInput] AS i
                        WHERE i.[TemplateId] = t.[Id] AND i.[Order] < 2147483647
                    ) AS candidates
                    WHERE existing.[LastOrder] = 2147483647
                      AND NOT EXISTS (
                          SELECT 1 FROM [TemplateCustomInput] AS i
                          WHERE i.[TemplateId] = t.[Id] AND i.[Order] = candidates.[AvailableOrder]
                      )
                    ORDER BY candidates.[AvailableOrder]
                ) AS gap
                WHERE NOT EXISTS (
                    SELECT 1 FROM [TemplateCustomInput] AS i
                    WHERE i.[TemplateId] = t.[Id] AND i.[SystemFieldKey] = 1
                );
                """);

            migrationBuilder.Sql("""
                INSERT INTO [AnonymousTemplateCustomInput]
                    ([Id], [AnonymousTemplateId], [LabelEn], [LabelAr], [Type], [IsRequired],
                     [Order], [IsActive], [CreatedByApplicationUserId], [CreatedOnUtc], [SystemFieldKey])
                SELECT NEWID(), t.[Id], N'Visit Time', N'تاريخ الزيارة', 3, 0,
                       CASE WHEN existing.[LastOrder] = 2147483647
                            THEN gap.[AvailableOrder] ELSE COALESCE(existing.[LastOrder], 0) + 1 END,
                       1, t.[CreatedByApplicationUserId], SYSUTCDATETIME(), 1
                FROM [AnonymousTemplate] AS t
                OUTER APPLY (
                    SELECT MAX(CONVERT(bigint, i.[Order])) AS [LastOrder]
                    FROM [AnonymousTemplateCustomInput] AS i
                    WHERE i.[AnonymousTemplateId] = t.[Id]
                ) AS existing
                OUTER APPLY (
                    SELECT TOP (1) candidates.[AvailableOrder]
                    FROM (
                        SELECT CONVERT(bigint, 1) AS [AvailableOrder]
                        UNION ALL
                        SELECT CONVERT(bigint, i.[Order]) + 1
                        FROM [AnonymousTemplateCustomInput] AS i
                        WHERE i.[AnonymousTemplateId] = t.[Id] AND i.[Order] < 2147483647
                    ) AS candidates
                    WHERE existing.[LastOrder] = 2147483647
                      AND NOT EXISTS (
                          SELECT 1 FROM [AnonymousTemplateCustomInput] AS i
                          WHERE i.[AnonymousTemplateId] = t.[Id] AND i.[Order] = candidates.[AvailableOrder]
                      )
                    ORDER BY candidates.[AvailableOrder]
                ) AS gap
                WHERE NOT EXISTS (
                    SELECT 1 FROM [AnonymousTemplateCustomInput] AS i
                    WHERE i.[AnonymousTemplateId] = t.[Id] AND i.[SystemFieldKey] = 1
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [SurveyResponseCustomInputValue] WHERE [TypeSnapshot] = 3;
                DELETE FROM [AnonymousSurveyResponseCustomInputValue] WHERE [TypeSnapshot] = 3;
                DELETE FROM [TemplateCustomInput] WHERE [Type] = 3;
                DELETE FROM [AnonymousTemplateCustomInput] WHERE [Type] = 3;
                """);

            migrationBuilder.DropIndex(
                name: "IX_TemplateCustomInput_TemplateId_SystemFieldKey",
                table: "TemplateCustomInput");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateCustomInput_Date_Validation",
                table: "TemplateCustomInput");

            migrationBuilder.DropIndex(
                name: "IX_SurveyResponseCustomInputValue_TemplateCustomInputId_DateValue_SurveyResponseId",
                table: "SurveyResponseCustomInputValue");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SRCIV_Date_Value",
                table: "SurveyResponseCustomInputValue");

            migrationBuilder.DropIndex(
                name: "IX_AnonymousTemplateCustomInput_AnonymousTemplateId_SystemFieldKey",
                table: "AnonymousTemplateCustomInput");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AnonymousTemplateCustomInput_Date_Validation",
                table: "AnonymousTemplateCustomInput");

            migrationBuilder.DropIndex(
                name: "IX_AnonymousSurveyResponseCustomInputValue_AnonymousTemplateCustomInputId_DateValue_AnonymousSurveyResponseId",
                table: "AnonymousSurveyResponseCustomInputValue");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ASRCIV_Date_Value",
                table: "AnonymousSurveyResponseCustomInputValue");

            migrationBuilder.DropColumn(
                name: "SystemFieldKey",
                table: "TemplateCustomInput");

            migrationBuilder.DropColumn(
                name: "DateValue",
                table: "SurveyResponseCustomInputValue");

            migrationBuilder.DropColumn(
                name: "SystemFieldKey",
                table: "AnonymousTemplateCustomInput");

            migrationBuilder.DropColumn(
                name: "DateValue",
                table: "AnonymousSurveyResponseCustomInputValue");
        }
    }
}

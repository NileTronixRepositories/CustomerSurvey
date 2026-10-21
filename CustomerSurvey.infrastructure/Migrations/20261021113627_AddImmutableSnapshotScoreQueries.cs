using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImmutableSnapshotScoreQueries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ResponseSnapshot_Source",
                table: "ResponseSnapshots");

            migrationBuilder.AddColumn<string>(
                name: "AllQuestionIds",
                table: "ResponseSnapshots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "AllScorePercentage",
                table: "ResponseSnapshots",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AllScoredItemsCount",
                table: "ResponseSnapshots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LowestQuestionIds",
                table: "ResponseSnapshots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "LowestScorePercentage",
                table: "ResponseSnapshots",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LowestScoredItemsCount",
                table: "ResponseSnapshots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RootQuestionIds",
                table: "ResponseSnapshots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RootScorePercentage",
                table: "ResponseSnapshots",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RootScoredItemsCount",
                table: "ResponseSnapshots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAvailabilityEvents_TemplateKind_TemplateId_IsBaseline",
                table: "TemplateAvailabilityEvents",
                columns: new[] { "TemplateKind", "TemplateId", "IsBaseline" },
                unique: true,
                filter: "[IsBaseline] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ResponseSnapshot_Source",
                table: "ResponseSnapshots",
                sql: "([TemplateKind] = 1 AND [SurveyResponseId] IS NOT NULL AND [SurveyResponseId] = [ResponseId] AND [AnonymousSurveyResponseId] IS NULL) OR ([TemplateKind] = 2 AND [AnonymousSurveyResponseId] IS NOT NULL AND [AnonymousSurveyResponseId] = [ResponseId] AND [SurveyResponseId] IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TemplateAvailabilityEvents_TemplateKind_TemplateId_IsBaseline",
                table: "TemplateAvailabilityEvents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ResponseSnapshot_Source",
                table: "ResponseSnapshots");

            migrationBuilder.DropColumn(
                name: "AllQuestionIds",
                table: "ResponseSnapshots");

            migrationBuilder.DropColumn(
                name: "AllScorePercentage",
                table: "ResponseSnapshots");

            migrationBuilder.DropColumn(
                name: "AllScoredItemsCount",
                table: "ResponseSnapshots");

            migrationBuilder.DropColumn(
                name: "LowestQuestionIds",
                table: "ResponseSnapshots");

            migrationBuilder.DropColumn(
                name: "LowestScorePercentage",
                table: "ResponseSnapshots");

            migrationBuilder.DropColumn(
                name: "LowestScoredItemsCount",
                table: "ResponseSnapshots");

            migrationBuilder.DropColumn(
                name: "RootQuestionIds",
                table: "ResponseSnapshots");

            migrationBuilder.DropColumn(
                name: "RootScorePercentage",
                table: "ResponseSnapshots");

            migrationBuilder.DropColumn(
                name: "RootScoredItemsCount",
                table: "ResponseSnapshots");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ResponseSnapshot_Source",
                table: "ResponseSnapshots",
                sql: "([TemplateKind] = 1 AND [SurveyResponseId] = [ResponseId] AND [AnonymousSurveyResponseId] IS NULL) OR ([TemplateKind] = 2 AND [AnonymousSurveyResponseId] = [ResponseId] AND [SurveyResponseId] IS NULL)");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddValueQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActualScore",
                table: "SurveyResponse",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxScore",
                table: "SurveyResponse",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ScorePercentage",
                table: "SurveyResponse",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "TextEn",
                table: "QuestionOption",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TextAr",
                table: "QuestionOption",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Value",
                table: "QuestionOption",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponse_OperatorId_TemplateId_SubmittedOnUtc",
                table: "SurveyResponse",
                columns: new[] { "OperatorId", "TemplateId", "SubmittedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponse_TemplateId_SubmittedOnUtc",
                table: "SurveyResponse",
                columns: new[] { "TemplateId", "SubmittedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOption_QuestionId_Order",
                table: "QuestionOption",
                columns: new[] { "QuestionId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SurveyResponse_OperatorId_TemplateId_SubmittedOnUtc",
                table: "SurveyResponse");

            migrationBuilder.DropIndex(
                name: "IX_SurveyResponse_TemplateId_SubmittedOnUtc",
                table: "SurveyResponse");

            migrationBuilder.DropIndex(
                name: "IX_QuestionOption_QuestionId_Order",
                table: "QuestionOption");

            migrationBuilder.DropColumn(
                name: "ActualScore",
                table: "SurveyResponse");

            migrationBuilder.DropColumn(
                name: "MaxScore",
                table: "SurveyResponse");

            migrationBuilder.DropColumn(
                name: "ScorePercentage",
                table: "SurveyResponse");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "QuestionOption");

            migrationBuilder.AlterColumn<string>(
                name: "TextEn",
                table: "QuestionOption",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "TextAr",
                table: "QuestionOption",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);
        }
    }
}

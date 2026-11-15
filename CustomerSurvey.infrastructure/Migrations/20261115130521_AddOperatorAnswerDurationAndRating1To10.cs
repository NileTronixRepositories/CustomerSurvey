using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOperatorAnswerDurationAndRating1To10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnswerDurationSeconds",
                table: "SurveyResponse",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating1To10Value",
                table: "SurveyAnswer",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating1To10Value",
                table: "AnonymousSurveyAnswer",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerDurationSeconds",
                table: "SurveyResponse");

            migrationBuilder.DropColumn(
                name: "Rating1To10Value",
                table: "SurveyAnswer");

            migrationBuilder.DropColumn(
                name: "Rating1To10Value",
                table: "AnonymousSurveyAnswer");
        }
    }
}

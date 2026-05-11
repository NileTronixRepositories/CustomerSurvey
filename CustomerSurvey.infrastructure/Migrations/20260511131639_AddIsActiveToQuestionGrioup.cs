using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToQuestionGrioup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "QuestionGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionGroup_BranchId_IsActive",
                table: "QuestionGroup",
                columns: new[] { "BranchId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestionGroup_BranchId_IsActive",
                table: "QuestionGroup");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "QuestionGroup");
        }
    }
}

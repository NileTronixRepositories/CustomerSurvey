using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStartWithToTemplateCustomInputs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StartWith",
                table: "TemplateCustomInput",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartWith",
                table: "AnonymousTemplateCustomInput",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateCustomInput_StartWith_StringOnly",
                table: "TemplateCustomInput",
                sql: "([Type] = 1) OR ([StartWith] IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AnonymousTemplateCustomInput_StartWith_StringOnly",
                table: "AnonymousTemplateCustomInput",
                sql: "([Type] = 1) OR ([StartWith] IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateCustomInput_StartWith_StringOnly",
                table: "TemplateCustomInput");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AnonymousTemplateCustomInput_StartWith_StringOnly",
                table: "AnonymousTemplateCustomInput");

            migrationBuilder.DropColumn(
                name: "StartWith",
                table: "TemplateCustomInput");

            migrationBuilder.DropColumn(
                name: "StartWith",
                table: "AnonymousTemplateCustomInput");
        }
    }
}

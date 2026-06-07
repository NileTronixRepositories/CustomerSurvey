using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordLifecycleToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFirstLogin",
                table: "ApplicationUser",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordChangedOnUtc",
                table: "ApplicationUser",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 6, 7, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.Sql(
                "UPDATE [ApplicationUser] SET [IsFirstLogin] = CAST(0 AS bit), [PasswordChangedOnUtc] = [CreatedOnUtc]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFirstLogin",
                table: "ApplicationUser");

            migrationBuilder.DropColumn(
                name: "PasswordChangedOnUtc",
                table: "ApplicationUser");
        }
    }
}

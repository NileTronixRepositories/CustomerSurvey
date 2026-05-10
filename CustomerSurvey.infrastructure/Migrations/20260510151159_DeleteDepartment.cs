using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Department_Branch_BranchId",
                table: "Department");

            migrationBuilder.DropIndex(
                name: "IX_Department_BranchId",
                table: "Department");

            migrationBuilder.DropIndex(
                name: "IX_Department_BranchId_Code",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Department");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Department",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Department",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Department_BranchId",
                table: "Department",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Department_BranchId_Code",
                table: "Department",
                columns: new[] { "BranchId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Branch_BranchId",
                table: "Department",
                column: "BranchId",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorizedSurveyResponseFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlagOfficers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlagOfficers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlagOfficers_ApplicationUser_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlagOfficers_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlagOfficers_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SurveyResponseFlags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SurveyResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Brief = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Answer = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BriefUpdatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BriefUpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnsweredByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AnsweredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyResponseFlags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyResponseFlags_ApplicationUser_AnsweredByApplicationUserId",
                        column: x => x.AnsweredByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SurveyResponseFlags_ApplicationUser_BriefUpdatedByApplicationUserId",
                        column: x => x.BriefUpdatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SurveyResponseFlags_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SurveyResponseFlags_SurveyResponse_SurveyResponseId",
                        column: x => x.SurveyResponseId,
                        principalTable: "SurveyResponse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlagOfficers_ApplicationUserId",
                table: "FlagOfficers",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlagOfficers_CreatedByApplicationUserId",
                table: "FlagOfficers",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FlagOfficers_DepartmentId",
                table: "FlagOfficers",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseFlags_AnsweredByApplicationUserId",
                table: "SurveyResponseFlags",
                column: "AnsweredByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseFlags_BriefUpdatedByApplicationUserId",
                table: "SurveyResponseFlags",
                column: "BriefUpdatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseFlags_CreatedByApplicationUserId",
                table: "SurveyResponseFlags",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseFlags_SurveyResponseId_Type",
                table: "SurveyResponseFlags",
                columns: new[] { "SurveyResponseId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseFlags_Type_SurveyResponseId",
                table: "SurveyResponseFlags",
                columns: new[] { "Type", "SurveyResponseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlagOfficers");

            migrationBuilder.DropTable(
                name: "SurveyResponseFlags");
        }
    }
}

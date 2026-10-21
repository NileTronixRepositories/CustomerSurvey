using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SurveyHistoryAndBranchEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BranchEvaluationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NeutralMinimum = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SatisfiedMinimum = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MediumRiskMinimum = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    HealthyMinimum = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchEvaluationSettings", x => x.Id);
                    table.CheckConstraint("CK_BranchEvaluation_Risk", "[MediumRiskMinimum] >= 0 AND [MediumRiskMinimum] < [HealthyMinimum] AND [HealthyMinimum] <= 100");
                    table.CheckConstraint("CK_BranchEvaluation_Satisfaction", "[NeutralMinimum] >= 0 AND [NeutralMinimum] < [SatisfiedMinimum] AND [SatisfiedMinimum] <= 100");
                    table.ForeignKey(
                        name: "FK_BranchEvaluationSettings_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResponseSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SurveyResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AnonymousSurveyResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateKind = table.Column<int>(type: "int", nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    Provenance = table.Column<int>(type: "int", nullable: false),
                    CapturedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseSnapshots", x => x.Id);
                    table.CheckConstraint("CK_ResponseSnapshot_Source", "([TemplateKind] = 1 AND [SurveyResponseId] = [ResponseId] AND [AnonymousSurveyResponseId] IS NULL) OR ([TemplateKind] = 2 AND [AnonymousSurveyResponseId] = [ResponseId] AND [SurveyResponseId] IS NULL)");
                    table.ForeignKey(
                        name: "FK_ResponseSnapshots_AnonymousSurveyResponse_AnonymousSurveyResponseId",
                        column: x => x.AnonymousSurveyResponseId,
                        principalTable: "AnonymousSurveyResponse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResponseSnapshots_SurveyResponse_SurveyResponseId",
                        column: x => x.SurveyResponseId,
                        principalTable: "SurveyResponse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TemplateAvailabilityEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateKind = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PreviousIsActive = table.Column<bool>(type: "bit", nullable: true),
                    PreviousActiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreviousExpireTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ActiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpireTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChangedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActorApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsBaseline = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateAvailabilityEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchEvaluationSettings_BranchId",
                table: "BranchEvaluationSettings",
                column: "BranchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResponseSnapshots_AnonymousSurveyResponseId",
                table: "ResponseSnapshots",
                column: "AnonymousSurveyResponseId",
                unique: true,
                filter: "[AnonymousSurveyResponseId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseSnapshots_SurveyResponseId",
                table: "ResponseSnapshots",
                column: "SurveyResponseId",
                unique: true,
                filter: "[SurveyResponseId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseSnapshots_TemplateKind_ResponseId",
                table: "ResponseSnapshots",
                columns: new[] { "TemplateKind", "ResponseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResponseSnapshots_TemplateKind_TemplateId",
                table: "ResponseSnapshots",
                columns: new[] { "TemplateKind", "TemplateId" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAvailabilityEvents_BranchId_ChangedOnUtc",
                table: "TemplateAvailabilityEvents",
                columns: new[] { "BranchId", "ChangedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAvailabilityEvents_TemplateKind_TemplateId_ChangedOnUtc",
                table: "TemplateAvailabilityEvents",
                columns: new[] { "TemplateKind", "TemplateId", "ChangedOnUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchEvaluationSettings");

            migrationBuilder.DropTable(
                name: "ResponseSnapshots");

            migrationBuilder.DropTable(
                name: "TemplateAvailabilityEvents");
        }
    }
}

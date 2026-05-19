using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingCustomInput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TemplateCustomInput",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LabelEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LabelAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    MinLength = table.Column<int>(type: "int", nullable: true),
                    MaxLength = table.Column<int>(type: "int", nullable: true),
                    MinValue = table.Column<int>(type: "int", nullable: true),
                    MaxValue = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateCustomInput", x => x.Id);
                    table.CheckConstraint("CK_TemplateCustomInput_Integer_Validation", "([Type] <> 2) OR ([MinLength] IS NULL AND [MaxLength] IS NULL)");
                    table.CheckConstraint("CK_TemplateCustomInput_Length_Range", "([MinLength] IS NULL OR [MaxLength] IS NULL OR [MaxLength] >= [MinLength])");
                    table.CheckConstraint("CK_TemplateCustomInput_Order_Positive", "[Order] > 0");
                    table.CheckConstraint("CK_TemplateCustomInput_String_Validation", "([Type] <> 1) OR ([MinValue] IS NULL AND [MaxValue] IS NULL)");
                    table.CheckConstraint("CK_TemplateCustomInput_Value_Range", "([MinValue] IS NULL OR [MaxValue] IS NULL OR [MaxValue] >= [MinValue])");
                    table.ForeignKey(
                        name: "FK_TemplateCustomInput_Template_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Template",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SurveyResponseCustomInputValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SurveyResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateCustomInputId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TypeSnapshot = table.Column<int>(type: "int", nullable: false),
                    StringValue = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    IntegerValue = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyResponseCustomInputValue", x => x.Id);
                    table.CheckConstraint("CK_SRCIV_Integer_Value", "([TypeSnapshot] <> 2) OR ([IntegerValue] IS NOT NULL AND [StringValue] IS NULL)");
                    table.CheckConstraint("CK_SRCIV_String_Value", "([TypeSnapshot] <> 1) OR ([StringValue] IS NOT NULL AND [IntegerValue] IS NULL)");
                    table.ForeignKey(
                        name: "FK_SurveyResponseCustomInputValue_SurveyResponse_SurveyResponseId",
                        column: x => x.SurveyResponseId,
                        principalTable: "SurveyResponse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SurveyResponseCustomInputValue_TemplateCustomInput_TemplateCustomInputId",
                        column: x => x.TemplateCustomInputId,
                        principalTable: "TemplateCustomInput",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseCustomInputValue_SurveyResponseId",
                table: "SurveyResponseCustomInputValue",
                column: "SurveyResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseCustomInputValue_SurveyResponseId_TemplateCustomInputId",
                table: "SurveyResponseCustomInputValue",
                columns: new[] { "SurveyResponseId", "TemplateCustomInputId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseCustomInputValue_TemplateCustomInputId",
                table: "SurveyResponseCustomInputValue",
                column: "TemplateCustomInputId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCustomInput_CreatedByApplicationUserId",
                table: "TemplateCustomInput",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCustomInput_TemplateId",
                table: "TemplateCustomInput",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCustomInput_TemplateId_IsActive_Order",
                table: "TemplateCustomInput",
                columns: new[] { "TemplateId", "IsActive", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCustomInput_TemplateId_Name",
                table: "TemplateCustomInput",
                columns: new[] { "TemplateId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurveyResponseCustomInputValue");

            migrationBuilder.DropTable(
                name: "TemplateCustomInput");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerSurvey.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnoTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnonymousTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpireTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PublicUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QrCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnonymousTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnonymousTemplate_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnonymousSurveyResponse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnonymousTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualScore = table.Column<int>(type: "int", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false),
                    ScorePercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnonymousSurveyResponse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnonymousSurveyResponse_AnonymousTemplate_AnonymousTemplateId",
                        column: x => x.AnonymousTemplateId,
                        principalTable: "AnonymousTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnonymousTemplateCustomInput",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnonymousTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_AnonymousTemplateCustomInput", x => x.Id);
                    table.CheckConstraint("CK_AnonymousTemplateCustomInput_Integer_Validation", "([Type] <> 2) OR ([MinLength] IS NULL AND [MaxLength] IS NULL)");
                    table.CheckConstraint("CK_AnonymousTemplateCustomInput_Length_Range", "([MinLength] IS NULL OR [MaxLength] IS NULL OR [MaxLength] >= [MinLength])");
                    table.CheckConstraint("CK_AnonymousTemplateCustomInput_Order_Positive", "[Order] > 0");
                    table.CheckConstraint("CK_AnonymousTemplateCustomInput_String_Validation", "([Type] <> 1) OR ([MinValue] IS NULL AND [MaxValue] IS NULL)");
                    table.CheckConstraint("CK_AnonymousTemplateCustomInput_Value_Range", "([MinValue] IS NULL OR [MaxValue] IS NULL OR [MaxValue] >= [MinValue])");
                    table.ForeignKey(
                        name: "FK_AnonymousTemplateCustomInput_AnonymousTemplate_AnonymousTemplateId",
                        column: x => x.AnonymousTemplateId,
                        principalTable: "AnonymousTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnonymousTemplateQuestion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnonymousTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnonymousTemplateQuestion", x => x.Id);
                    table.CheckConstraint("CK_AnonymousTemplateQuestion_Order_Positive", "[Order] > 0");
                    table.ForeignKey(
                        name: "FK_AnonymousTemplateQuestion_AnonymousTemplate_AnonymousTemplateId",
                        column: x => x.AnonymousTemplateId,
                        principalTable: "AnonymousTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnonymousTemplateQuestion_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnonymousSurveyResponseCustomInputValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnonymousSurveyResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnonymousTemplateCustomInputId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TypeSnapshot = table.Column<int>(type: "int", nullable: false),
                    StringValue = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    IntegerValue = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnonymousSurveyResponseCustomInputValue", x => x.Id);
                    table.CheckConstraint("CK_ASRCIV_Integer_Value", "([TypeSnapshot] <> 2) OR ([IntegerValue] IS NOT NULL AND [StringValue] IS NULL)");
                    table.CheckConstraint("CK_ASRCIV_String_Value", "([TypeSnapshot] <> 1) OR ([StringValue] IS NOT NULL AND [IntegerValue] IS NULL)");
                    table.ForeignKey(
                        name: "FK_AnonymousSurveyResponseCustomInputValue_AnonymousSurveyResponse_AnonymousSurveyResponseId",
                        column: x => x.AnonymousSurveyResponseId,
                        principalTable: "AnonymousSurveyResponse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnonymousSurveyResponseCustomInputValue_AnonymousTemplateCustomInput_AnonymousTemplateCustomInputId",
                        column: x => x.AnonymousTemplateCustomInputId,
                        principalTable: "AnonymousTemplateCustomInput",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnonymousSurveyAnswer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnonymousSurveyResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnonymousTemplateQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    SelectedQuestionOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StarRatingValue = table.Column<int>(type: "int", nullable: true),
                    SmileValue = table.Column<int>(type: "int", nullable: true),
                    TextAnswer = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    VoiceFileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnonymousSurveyAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnonymousSurveyAnswer_AnonymousSurveyResponse_AnonymousSurveyResponseId",
                        column: x => x.AnonymousSurveyResponseId,
                        principalTable: "AnonymousSurveyResponse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnonymousSurveyAnswer_AnonymousTemplateQuestion_AnonymousTemplateQuestionId",
                        column: x => x.AnonymousTemplateQuestionId,
                        principalTable: "AnonymousTemplateQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnonymousSurveyAnswer_QuestionOption_SelectedQuestionOptionId",
                        column: x => x.SelectedQuestionOptionId,
                        principalTable: "QuestionOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnonymousSurveyAnswer_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnonymousTemplateQuestionCondition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnonymousTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentAnonymousTemplateQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChildAnonymousTemplateQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TriggerType = table.Column<int>(type: "int", nullable: false),
                    SelectedQuestionOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TriggerValue = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnonymousTemplateQuestionCondition", x => x.Id);
                    table.CheckConstraint("CK_AnonymousTemplateQuestionCondition_Not_Same_Question", "[ParentAnonymousTemplateQuestionId] <> [ChildAnonymousTemplateQuestionId]");
                    table.CheckConstraint("CK_AnonymousTemplateQuestionCondition_Order_Positive", "[Order] > 0");
                    table.CheckConstraint("CK_AnonymousTemplateQuestionCondition_RatingOrSmile", "([TriggerType] NOT IN (2, 3)) OR ([SelectedQuestionOptionId] IS NULL AND [TriggerValue] BETWEEN 1 AND 5)");
                    table.CheckConstraint("CK_AnonymousTemplateQuestionCondition_SingleChoice", "([TriggerType] <> 1) OR ([SelectedQuestionOptionId] IS NOT NULL AND [TriggerValue] IS NULL)");
                    table.ForeignKey(
                        name: "FK_AnonymousTemplateQuestionCondition_AnonymousTemplateQuestion_ChildAnonymousTemplateQuestionId",
                        column: x => x.ChildAnonymousTemplateQuestionId,
                        principalTable: "AnonymousTemplateQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnonymousTemplateQuestionCondition_AnonymousTemplateQuestion_ParentAnonymousTemplateQuestionId",
                        column: x => x.ParentAnonymousTemplateQuestionId,
                        principalTable: "AnonymousTemplateQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnonymousTemplateQuestionCondition_AnonymousTemplate_AnonymousTemplateId",
                        column: x => x.AnonymousTemplateId,
                        principalTable: "AnonymousTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnonymousTemplateQuestionCondition_QuestionOption_SelectedQuestionOptionId",
                        column: x => x.SelectedQuestionOptionId,
                        principalTable: "QuestionOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousSurveyAnswer_AnonymousSurveyResponseId_AnonymousTemplateQuestionId",
                table: "AnonymousSurveyAnswer",
                columns: new[] { "AnonymousSurveyResponseId", "AnonymousTemplateQuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousSurveyAnswer_AnonymousTemplateQuestionId",
                table: "AnonymousSurveyAnswer",
                column: "AnonymousTemplateQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousSurveyAnswer_QuestionId",
                table: "AnonymousSurveyAnswer",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousSurveyAnswer_SelectedQuestionOptionId",
                table: "AnonymousSurveyAnswer",
                column: "SelectedQuestionOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousSurveyResponse_AnonymousTemplateId",
                table: "AnonymousSurveyResponse",
                column: "AnonymousTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousSurveyResponse_AnonymousTemplateId_SubmittedOnUtc",
                table: "AnonymousSurveyResponse",
                columns: new[] { "AnonymousTemplateId", "SubmittedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousSurveyResponse_SubmittedOnUtc",
                table: "AnonymousSurveyResponse",
                column: "SubmittedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousSurveyResponseCustomInputValue_AnonymousSurveyResponseId",
                table: "AnonymousSurveyResponseCustomInputValue",
                column: "AnonymousSurveyResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousSurveyResponseCustomInputValue_AnonymousSurveyResponseId_AnonymousTemplateCustomInputId",
                table: "AnonymousSurveyResponseCustomInputValue",
                columns: new[] { "AnonymousSurveyResponseId", "AnonymousTemplateCustomInputId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousSurveyResponseCustomInputValue_AnonymousTemplateCustomInputId",
                table: "AnonymousSurveyResponseCustomInputValue",
                column: "AnonymousTemplateCustomInputId");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplate_BranchId",
                table: "AnonymousTemplate",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateCustomInput_AnonymousTemplateId",
                table: "AnonymousTemplateCustomInput",
                column: "AnonymousTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateCustomInput_AnonymousTemplateId_IsActive_Order",
                table: "AnonymousTemplateCustomInput",
                columns: new[] { "AnonymousTemplateId", "IsActive", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateCustomInput_AnonymousTemplateId_Name",
                table: "AnonymousTemplateCustomInput",
                columns: new[] { "AnonymousTemplateId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateQuestion_AnonymousTemplateId",
                table: "AnonymousTemplateQuestion",
                column: "AnonymousTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateQuestion_AnonymousTemplateId_Order",
                table: "AnonymousTemplateQuestion",
                columns: new[] { "AnonymousTemplateId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateQuestion_AnonymousTemplateId_QuestionId",
                table: "AnonymousTemplateQuestion",
                columns: new[] { "AnonymousTemplateId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateQuestion_QuestionId",
                table: "AnonymousTemplateQuestion",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateQuestionCondition_AnonymousTemplateId",
                table: "AnonymousTemplateQuestionCondition",
                column: "AnonymousTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateQuestionCondition_AnonymousTemplateId_ParentAnonymousTemplateQuestionId_ChildAnonymousTemplateQuestionId_Tr~",
                table: "AnonymousTemplateQuestionCondition",
                columns: new[] { "AnonymousTemplateId", "ParentAnonymousTemplateQuestionId", "ChildAnonymousTemplateQuestionId", "TriggerType", "SelectedQuestionOptionId", "TriggerValue" },
                unique: true,
                filter: "[SelectedQuestionOptionId] IS NOT NULL AND [TriggerValue] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateQuestionCondition_ChildAnonymousTemplateQuestionId",
                table: "AnonymousTemplateQuestionCondition",
                column: "ChildAnonymousTemplateQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateQuestionCondition_ParentAnonymousTemplateQuestionId",
                table: "AnonymousTemplateQuestionCondition",
                column: "ParentAnonymousTemplateQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousTemplateQuestionCondition_SelectedQuestionOptionId",
                table: "AnonymousTemplateQuestionCondition",
                column: "SelectedQuestionOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnonymousSurveyAnswer");

            migrationBuilder.DropTable(
                name: "AnonymousSurveyResponseCustomInputValue");

            migrationBuilder.DropTable(
                name: "AnonymousTemplateQuestionCondition");

            migrationBuilder.DropTable(
                name: "AnonymousSurveyResponse");

            migrationBuilder.DropTable(
                name: "AnonymousTemplateCustomInput");

            migrationBuilder.DropTable(
                name: "AnonymousTemplateQuestion");

            migrationBuilder.DropTable(
                name: "AnonymousTemplate");
        }
    }
}

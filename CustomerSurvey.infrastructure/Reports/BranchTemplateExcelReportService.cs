using BuildingBlock.Domain.Results;
using ClosedXML.Excel;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using System.Text.RegularExpressions;

namespace CustomerSurvey.infrastructure.Reports;

internal sealed partial class BranchTemplateExcelReportService : IBranchTemplateExcelReportService
{
    private const int ExcelMaximumColumns = 16_384;
    private const int DataHeaderRow = 3;
    private const int MaximumDataRowsPerSheet = 1_048_576 - DataHeaderRow;
    private const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private static readonly XLColor TitleColor = XLColor.FromHtml("#17365D");
    private static readonly XLColor HeaderColor = XLColor.FromHtml("#1F4E78");
    private static readonly XLColor SectionColor = XLColor.FromHtml("#D9EAF7");
    private static readonly XLColor LightBorderColor = XLColor.FromHtml("#D9E2F3");

    private readonly IBranchTemplatesPdfReportService _branchTemplatesReportService;

    public BranchTemplateExcelReportService(
        IBranchTemplatesPdfReportService branchTemplatesReportService)
    {
        _branchTemplatesReportService = branchTemplatesReportService
            ?? throw new ArgumentNullException(nameof(branchTemplatesReportService));
    }

    public async Task<Result<BranchTemplateExcelReportFile>> GenerateAsync(
        BranchTemplatesPdfReportRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.TemplateId.HasValue || request.TemplateId.Value == Guid.Empty)
        {
            return Result<BranchTemplateExcelReportFile>.Fail(new Error(
                Code: "Reports.TemplateExcel.TemplateIdRequired",
                Message: ErrorMessage.GetTemplateDetails_TemplateId_Required,
                Type: ErrorType.Validation));
        }

        var modelResult = await _branchTemplatesReportService.BuildReportModelAsync(
            request with { IncludeResponseDetails = true },
            cancellationToken);

        if (modelResult.IsFailure)
        {
            return Result<BranchTemplateExcelReportFile>.Fail(modelResult.Errors);
        }

        var model = modelResult.Value;

        if (model.Templates.Count != 1)
        {
            return Result<BranchTemplateExcelReportFile>.Fail(new Error(
                Code: "Reports.TemplateExcel.SingleTemplateRequired",
                Message: "Excel report generation requires exactly one accessible template.",
                Type: ErrorType.Validation));
        }

        using var workbook = BuildWorkbook(model);
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return Result<BranchTemplateExcelReportFile>.Ok(
            new BranchTemplateExcelReportFile
            {
                FileName = BuildFileName(model),
                ContentType = ContentType,
                Content = stream.ToArray()
            });
    }

    private static XLWorkbook BuildWorkbook(BranchTemplatesPdfReportModel model)
    {
        var workbook = new XLWorkbook();
        workbook.Properties.Title = $"{model.SelectedTemplateName} analytical report";
        workbook.Properties.Subject = "Customer survey template report";
        workbook.Properties.Author = model.GeneratedBy;

        AddSummarySheet(workbook, model);
        AddResponseMatrixSheets(workbook, model);
        AddResponsesSheets(workbook, model);
        AddAnswersSheets(workbook, model);
        AddCustomInputsSheets(workbook, model);
        AddQuestionAnalysisSheets(workbook, model);
        AddRankingSheets(workbook, model, worst: true);
        AddRankingSheets(workbook, model, worst: false);

        return workbook;
    }

    private static void AddSummarySheet(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model)
    {
        var worksheet = workbook.Worksheets.Add("01 - Summary");
        ApplyWorksheetDefaults(worksheet);

        var template = model.Templates.Single();
        var isArabic = model.IsArabic;

        worksheet.Cell("A1").Value = "Customer Survey Template Report";
        StyleTitle(worksheet.Cell("A1"));

        WriteSummarySection(
            worksheet,
            startRow: 3,
            title: "Applied Filters",
            new (string Label, object? Value, string? NumberFormat)[]
            {
                ("Branch", model.BranchName, null),
                ("Template", template.DisplayName(isArabic), null),
                ("Template Kind", template.DisplayKind(isArabic), null),
                ("From Date", model.FromDate.ToDateTime(TimeOnly.MinValue), "yyyy-mm-dd"),
                ("To Date", model.ToDate.ToDateTime(TimeOnly.MinValue), "yyyy-mm-dd"),
                ("Score Calculation Mode", model.ScoreCalculationMode.ToString(), null),
                ("Top Questions Count", model.TopWorstQuestionsCount, "0"),
                ("Worst Questions Threshold", model.WorstQuestionsMaxScorePercentage / 100m, "0.00%"),
                ("Best Questions Threshold", model.BestQuestionsMinScorePercentage / 100m, "0.00%"),
                ("Language", model.Language, null),
                ("Generated By", model.GeneratedBy, null),
                ("Generated At UTC", model.GeneratedAtUtc, "yyyy-mm-dd hh:mm:ss")
            });

        var responsesWithScore = model.Responses.Count(x => x.IsScored);

        WriteSummarySection(
            worksheet,
            startRow: 18,
            title: "Summary Metrics",
            new (string Label, object? Value, string? NumberFormat)[]
            {
                ("Template Name", template.DisplayName(isArabic), null),
                ("Template Kind", template.DisplayKind(isArabic), null),
                ("Template Status", template.Status, null),
                ("Total Questions", template.TotalQuestions, "0"),
                ("Root Questions", template.RootQuestions, "0"),
                ("Conditional Questions", template.ConditionalQuestions, "0"),
                ("Total Responses", template.TotalResponses, "0"),
                ("Total Answers", template.TotalAnswers, "0"),
                ("Total Scored Answers", template.TotalScoredAnswers, "0"),
                ("Average Score / 5", template.AverageScoreValue, "0.00"),
                ("Average Satisfaction %", ToExcelPercentage(template.AverageScorePercentage), "0.00%"),
                ("Responses With Score", responsesWithScore, "0"),
                ("Responses Without Score", Math.Max(0, model.Responses.Count - responsesWithScore), "0")
            });

        var explanationRow = 34;
        worksheet.Cell(explanationRow, 1).Value = "Scoring Explanation";
        StyleSectionHeader(worksheet.Range(explanationRow, 1, explanationRow, 2));
        worksheet.Cell(explanationRow + 1, 1).Value = model.ScoreCalculationMode == ScoreCalculationMode.RootQuestions
            ? "Only answered, scorable root questions participate in each response score."
            : "Scoring follows each matched condition path and uses the last valid scorable question selected by the existing report scoring rules.";
        worksheet.Range(explanationRow + 1, 1, explanationRow + 2, 2).Merge();
        worksheet.Cell(explanationRow + 1, 1).Style.Alignment.WrapText = true;
        worksheet.Cell(explanationRow + 1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

        worksheet.Column(1).Width = 31;
        worksheet.Column(2).Width = 58;
        worksheet.Row(explanationRow + 1).Height = 36;
        worksheet.SheetView.ZoomScale = 90;
    }

    private static void AddResponseMatrixSheets(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model)
    {
        var isArabic = model.IsArabic;
        var customInputColumns = BuildMatrixCustomInputColumns(model);
        var questionColumns = BuildMatrixQuestionColumns(model);

        var headers = new List<string>
        {
            "#",
            "Survey Response Id",
            "Submitted On UTC",
            "Template Kind",
            "Operator Id",
            "Operator Name",
            "Scoring Status",
            "Score Value",
            "Max Score",
            "Score Percentage"
        };

        headers.AddRange(customInputColumns.Select(x => x.Header));
        headers.AddRange(questionColumns.Select(x => x.Header));
        headers = MakeHeadersUnique(headers);

        EnsureColumnLimit(headers.Count, "Response Matrix");

        var rows = model.Responses
            .Select((response, index) =>
            {
                var values = new List<object?>
                {
                    index + 1,
                    response.ResponseId.ToString(),
                    response.SubmittedOnUtc,
                    DisplayTemplateKind(response.TemplateKind, isArabic),
                    response.OperatorId?.ToString(),
                    response.DisplayOperatorName(isArabic),
                    response.IsScored ? "Scored" : "Not Scored",
                    response.IsScored ? response.AverageScoreValue : null,
                    response.IsScored ? response.MaxScore : null,
                    response.IsScored ? ToExcelPercentage(response.ScorePercentage) : null
                };

                var customInputsById = response.CustomInputs
                    .GroupBy(x => x.CustomInputId)
                    .ToDictionary(x => x.Key, x => x.First());

                foreach (var customColumn in customInputColumns)
                {
                    customInputsById.TryGetValue(customColumn.CustomInputId, out var customInput);
                    values.Add(ToTypedCustomInputValue(customInput));
                }

                var answersByTemplateQuestionId = response.Answers
                    .Where(x => x.TemplateQuestionId.HasValue)
                    .GroupBy(x => x.TemplateQuestionId!.Value)
                    .ToDictionary(x => x.Key, x => x.First());

                foreach (var questionColumn in questionColumns)
                {
                    answersByTemplateQuestionId.TryGetValue(
                        questionColumn.TemplateQuestionId,
                        out var answer);
                    values.Add(answer is null ? null : ResolveLocalizedAnswerValue(answer, isArabic));
                }

                return (IReadOnlyList<object?>)values;
            })
            .ToArray();

        var percentageColumn = headers.IndexOf("Score Percentage") + 1;
        var wrapColumns = Enumerable.Range(11, Math.Max(0, headers.Count - 10)).ToHashSet();
        var textColumns = customInputColumns
            .Select((column, index) => new { column.Type, ExcelColumn = 11 + index })
            .Where(x => x.Type == TemplateCustomInputType.String)
            .Select(x => x.ExcelColumn)
            .ToHashSet();

        AddDataSheets(
            workbook,
            "02 - Response Matrix",
            "ResponseMatrixTable",
            headers,
            rows,
            freezeColumns: 10,
            percentageColumns: new HashSet<int> { percentageColumn },
            wrapColumns,
            textColumns);
    }

    private static void AddResponsesSheets(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model)
    {
        var isArabic = model.IsArabic;
        var headers = new[]
        {
            "#",
            "Survey Response Id",
            "Template Id",
            "Template Name",
            "Template Kind",
            "Submitted On UTC",
            "Operator Id",
            "Operator Name",
            "Scoring Status",
            "Scored Items Count",
            "Average Score Value",
            "Max Score",
            "Score Percentage"
        };

        var rows = model.Responses
            .Select((response, index) => (IReadOnlyList<object?>)new object?[]
            {
                index + 1,
                response.ResponseId.ToString(),
                response.TemplateId.ToString(),
                response.DisplayTemplateName(isArabic),
                DisplayTemplateKind(response.TemplateKind, isArabic),
                response.SubmittedOnUtc,
                response.OperatorId?.ToString(),
                response.DisplayOperatorName(isArabic),
                response.IsScored ? "Scored" : "Not Scored",
                response.IsScored ? response.ScoredItemsCount : 0,
                response.IsScored ? response.AverageScoreValue : null,
                response.IsScored ? response.MaxScore : null,
                response.IsScored ? ToExcelPercentage(response.ScorePercentage) : null
            })
            .ToArray();

        AddDataSheets(
            workbook,
            "03 - Responses",
            "ResponsesTable",
            headers,
            rows,
            freezeColumns: 3,
            percentageColumns: new HashSet<int> { 13 },
            wrapColumns: new HashSet<int>());
    }

    private static void AddAnswersSheets(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model)
    {
        var isArabic = model.IsArabic;
        var headers = new[]
        {
            "Survey Response Id",
            "Submitted On UTC",
            "Operator Name",
            "Template Question Id",
            "Question Id",
            "Question Order",
            "Question",
            "Question Type",
            "Question Level",
            "Parent Trigger",
            "Answer",
            "Selected Option Id",
            "Selected Option",
            "Option Value",
            "Star Rating",
            "Smile Value",
            "Complaint Text",
            "Voice File",
            "Voice URL/Path",
            "Image File",
            "Image URL/Path",
            "Is Scorable",
            "Score Value",
            "Included In Score",
            "Score Inclusion Reason"
        };

        var rows = model.Responses
            .SelectMany(response => response.Answers.Select(answer => (IReadOnlyList<object?>)new object?[]
            {
                response.ResponseId.ToString(),
                response.SubmittedOnUtc,
                response.DisplayOperatorName(isArabic),
                answer.TemplateQuestionId?.ToString(),
                answer.QuestionId.ToString(),
                answer.QuestionOrder,
                answer.DisplayQuestionText(isArabic),
                answer.QuestionType.ToString(),
                answer.IsRootQuestion ? (isArabic ? "رئيسي" : "Root") : (isArabic ? "شرطي" : "Conditional"),
                answer.DisplayParentTrigger(isArabic),
                ResolveLocalizedAnswerValue(answer, isArabic),
                answer.SelectedQuestionOptionId?.ToString(),
                answer.DisplaySelectedOption(isArabic),
                answer.SelectedOptionValue,
                answer.StarRatingValue,
                answer.SmileValue,
                answer.TextAnswer,
                answer.VoiceFileName,
                answer.VoiceFilePath,
                answer.ImageFileName,
                answer.ImageFilePath,
                answer.IsScorable,
                answer.ScoreValue,
                answer.IncludedInScore,
                answer.ScoreInclusionReason
            }))
            .ToArray();

        AddDataSheets(
            workbook,
            "04 - Answers",
            "AnswersTable",
            headers,
            rows,
            freezeColumns: 2,
            percentageColumns: new HashSet<int>(),
            wrapColumns: new HashSet<int> { 7, 10, 11, 17, 19, 21, 25 });
    }

    private static void AddCustomInputsSheets(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model)
    {
        var isArabic = model.IsArabic;
        var headers = new[]
        {
            "Survey Response Id",
            "Submitted On UTC",
            "Operator Name",
            "Custom Input Id",
            "Custom Input Name",
            "Custom Input Type",
            "String Value",
            "Integer Value",
            "Display Value"
        };

        var rows = model.Responses
            .SelectMany(response => response.CustomInputs.Select(customInput => (IReadOnlyList<object?>)new object?[]
            {
                response.ResponseId.ToString(),
                response.SubmittedOnUtc,
                response.DisplayOperatorName(isArabic),
                customInput.CustomInputId.ToString(),
                customInput.Name,
                customInput.Type.ToString(),
                customInput.StringValue,
                customInput.IntegerValue,
                customInput.DisplayValue
            }))
            .ToArray();

        AddDataSheets(
            workbook,
            "05 - Custom Inputs",
            "CustomInputsTable",
            headers,
            rows,
            freezeColumns: 2,
            percentageColumns: new HashSet<int>(),
            wrapColumns: new HashSet<int> { 5, 7, 9 },
            textColumns: new HashSet<int> { 7, 9 });
    }

    private static void AddQuestionAnalysisSheets(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model)
    {
        var isArabic = model.IsArabic;
        var headers = new[]
        {
            "Template Question Id",
            "Question Id",
            "Question Order",
            "Question",
            "Question Type",
            "Level",
            "Parent Trigger",
            "Total Answers",
            "Skipped Count",
            "Average Value",
            "Score Average Value",
            "Score Average Percentage",
            "Included In Score"
        };

        var rows = model.Questions
            .OrderBy(x => x.QuestionOrder)
            .Select(question => (IReadOnlyList<object?>)new object?[]
            {
                question.TemplateQuestionId.ToString(),
                question.QuestionId.ToString(),
                question.QuestionOrder,
                question.DisplayQuestionText(isArabic),
                question.QuestionType,
                question.DisplayLevel(isArabic),
                question.DisplayParentTrigger(isArabic),
                question.TotalAnswers,
                question.SkippedCount,
                question.AverageValue,
                question.ScoreAverageValue,
                ToExcelPercentage(question.ScoreAveragePercentage),
                question.IsScoreIncluded
            })
            .ToArray();

        AddDataSheets(
            workbook,
            "06 - Question Analysis",
            "QuestionAnalysisTable",
            headers,
            rows,
            freezeColumns: 3,
            percentageColumns: new HashSet<int> { 12 },
            wrapColumns: new HashSet<int> { 4, 7 });
    }

    private static void AddRankingSheets(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model,
        bool worst)
    {
        var isArabic = model.IsArabic;
        var headers = new[]
        {
            "Rank",
            "Question",
            "Question Type",
            "Level",
            "Answers",
            "Average Score",
            "Satisfaction %"
        };

        var ranking = worst ? model.WorstQuestions : model.BestQuestions;
        var rows = ranking
            .Select(question => (IReadOnlyList<object?>)new object?[]
            {
                question.Rank,
                question.DisplayQuestionText(isArabic),
                question.QuestionType,
                question.DisplayLevel(isArabic),
                question.TotalAnswers,
                question.AverageScoreValue,
                question.SatisfactionPercentage / 100m
            })
            .ToArray();

        AddDataSheets(
            workbook,
            worst ? "07 - Worst Questions" : "08 - Best Questions",
            worst ? "WorstQuestionsTable" : "BestQuestionsTable",
            headers,
            rows,
            freezeColumns: 1,
            percentageColumns: new HashSet<int> { 7 },
            wrapColumns: new HashSet<int> { 2 });
    }

    private static void AddDataSheets(
        XLWorkbook workbook,
        string baseSheetName,
        string baseTableName,
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<object?>> rows,
        int freezeColumns,
        IReadOnlySet<int> percentageColumns,
        IReadOnlySet<int> wrapColumns,
        IReadOnlySet<int>? textColumns = null)
    {
        EnsureColumnLimit(headers.Count, baseSheetName);

        var sheetCount = Math.Max(1, (int)Math.Ceiling(rows.Count / (double)MaximumDataRowsPerSheet));

        for (var sheetIndex = 0; sheetIndex < sheetCount; sheetIndex++)
        {
            var sheetName = sheetIndex == 0
                ? baseSheetName
                : $"{baseSheetName} {sheetIndex + 1}";
            var worksheet = workbook.Worksheets.Add(sheetName);
            ApplyWorksheetDefaults(worksheet);

            worksheet.Cell(1, 1).Value = baseSheetName[(baseSheetName.IndexOf('-') + 1)..].Trim();
            StyleTitle(worksheet.Cell(1, 1));

            for (var column = 0; column < headers.Count; column++)
            {
                worksheet.Cell(DataHeaderRow, column + 1).Value = headers[column];
            }

            var headerRange = worksheet.Range(DataHeaderRow, 1, DataHeaderRow, headers.Count);
            StyleTableHeader(headerRange);

            var startIndex = sheetIndex * MaximumDataRowsPerSheet;
            var rowCount = Math.Min(MaximumDataRowsPerSheet, rows.Count - startIndex);

            for (var rowOffset = 0; rowOffset < rowCount; rowOffset++)
            {
                var values = rows[startIndex + rowOffset];
                var excelRow = DataHeaderRow + 1 + rowOffset;

                for (var column = 0; column < headers.Count; column++)
                {
                    SetCellValue(
                        worksheet.Cell(excelRow, column + 1),
                        column < values.Count ? values[column] : null);
                }
            }

            var lastRow = DataHeaderRow + rowCount;

            if (rowCount > 0)
            {
                var table = worksheet
                    .Range(DataHeaderRow, 1, lastRow, headers.Count)
                    .CreateTable($"{baseTableName}{sheetIndex + 1}");
                table.Theme = XLTableTheme.TableStyleMedium2;
                table.ShowAutoFilter = true;
                table.ShowRowStripes = true;
            }
            else
            {
                headerRange.SetAutoFilter();
            }

            worksheet.SheetView.FreezeRows(DataHeaderRow);

            if (freezeColumns > 0)
            {
                worksheet.SheetView.FreezeColumns(Math.Min(freezeColumns, headers.Count));
            }

            for (var column = 1; column <= headers.Count; column++)
            {
                worksheet.Column(column).Width = ResolveColumnWidth(headers[column - 1]);

                if (wrapColumns.Contains(column))
                {
                    worksheet.Column(column).Style.Alignment.WrapText = true;
                    worksheet.Column(column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                }

                if (textColumns?.Contains(column) == true && rowCount > 0)
                {
                    worksheet.Range(DataHeaderRow + 1, column, lastRow, column)
                        .Style.NumberFormat.Format = "@";
                }

                if (percentageColumns.Contains(column) && rowCount > 0)
                {
                    var percentageRange = worksheet.Range(DataHeaderRow + 1, column, lastRow, column);
                    percentageRange.Style.NumberFormat.Format = "0.00%";
                    AddScoreConditionalFormatting(percentageRange);
                }
            }

            ApplyDataFormats(worksheet, headers, rowCount);
            worksheet.SheetView.ZoomScale = 90;
        }
    }

    private static void ApplyDataFormats(
        IXLWorksheet worksheet,
        IReadOnlyList<string> headers,
        int rowCount)
    {
        if (rowCount == 0)
        {
            return;
        }

        var lastRow = DataHeaderRow + rowCount;

        for (var index = 0; index < headers.Count; index++)
        {
            var column = index + 1;
            var header = headers[index];
            var range = worksheet.Range(DataHeaderRow + 1, column, lastRow, column);

            if (header == "Submitted On UTC")
            {
                range.Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
            }
            else if (header.Contains("Score", StringComparison.OrdinalIgnoreCase) &&
                     !header.Contains("Percentage", StringComparison.OrdinalIgnoreCase) &&
                     header != "Scoring Status" &&
                     header != "Scored Items Count")
            {
                range.Style.NumberFormat.Format = "0.00";
            }
        }
    }

    private static void AddScoreConditionalFormatting(IXLRange range)
    {
        var firstAddress = range.FirstCell().Address.ToStringRelative();

        AddScoreConditionalFormattingRule(
            range,
            $"=AND(ISNUMBER({firstAddress}),{firstAddress}>=0.8)",
            "#C6EFCE");
        AddScoreConditionalFormattingRule(
            range,
            $"=AND(ISNUMBER({firstAddress}),{firstAddress}>=0.6,{firstAddress}<0.8)",
            "#DDEBF7");
        AddScoreConditionalFormattingRule(
            range,
            $"=AND(ISNUMBER({firstAddress}),{firstAddress}>=0.4,{firstAddress}<0.6)",
            "#FFF2CC");
        AddScoreConditionalFormattingRule(
            range,
            $"=AND(ISNUMBER({firstAddress}),{firstAddress}>=0.2,{firstAddress}<0.4)",
            "#FCE4D6");
        AddScoreConditionalFormattingRule(
            range,
            $"=AND(ISNUMBER({firstAddress}),{firstAddress}<0.2)",
            "#FFC7CE");
    }

    private static void AddScoreConditionalFormattingRule(
        IXLRange range,
        string formula,
        string colorHex)
    {
        range.AddConditionalFormat()
            .WhenIsTrue(formula)
            .Fill.SetBackgroundColor(XLColor.FromHtml(colorHex));
    }

    private static IReadOnlyList<MatrixCustomInputColumn> BuildMatrixCustomInputColumns(
        BranchTemplatesPdfReportModel model)
    {
        var current = model.CustomInputDefinitions
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Name)
            .Select(x => new MatrixCustomInputColumn(
                x.CustomInputId,
                x.DisplayName(model.IsArabic),
                x.Type))
            .ToList();

        var currentIds = current.Select(x => x.CustomInputId).ToHashSet();
        var historical = model.Responses
            .SelectMany(x => x.CustomInputs)
            .Where(x => !currentIds.Contains(x.CustomInputId))
            .GroupBy(x => x.CustomInputId)
            .Select(x => x.OrderBy(value => value.Order ?? int.MaxValue).First())
            .OrderBy(x => x.Order ?? int.MaxValue)
            .ThenBy(x => x.Name)
            .Select(x => new MatrixCustomInputColumn(
                x.CustomInputId,
                x.Name,
                x.Type));

        current.AddRange(historical);
        return current;
    }

    private static IReadOnlyList<MatrixQuestionColumn> BuildMatrixQuestionColumns(
        BranchTemplatesPdfReportModel model)
        => model.Questions
            .OrderBy(x => x.QuestionOrder)
            .ThenBy(x => x.QuestionTextEn)
            .Select(x => new MatrixQuestionColumn(
                x.TemplateQuestionId,
                x.DisplayQuestionText(model.IsArabic)))
            .ToArray();

    private static List<string> MakeHeadersUnique(IReadOnlyList<string> headers)
    {
        var result = new List<string>(headers.Count);
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawHeader in headers)
        {
            var header = string.IsNullOrWhiteSpace(rawHeader) ? "Unnamed" : rawHeader.Trim();
            counts.TryGetValue(header, out var count);
            count++;
            counts[header] = count;
            result.Add(count == 1 ? header : $"{header} ({count})");
        }

        return result;
    }

    private static void WriteSummarySection(
        IXLWorksheet worksheet,
        int startRow,
        string title,
        IReadOnlyList<(string Label, object? Value, string? NumberFormat)> rows)
    {
        worksheet.Cell(startRow, 1).Value = title;
        StyleSectionHeader(worksheet.Range(startRow, 1, startRow, 2));

        for (var index = 0; index < rows.Count; index++)
        {
            var row = startRow + 1 + index;
            worksheet.Cell(row, 1).Value = rows[index].Label;
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            SetCellValue(worksheet.Cell(row, 2), rows[index].Value);

            if (!string.IsNullOrWhiteSpace(rows[index].NumberFormat))
            {
                worksheet.Cell(row, 2).Style.NumberFormat.Format = rows[index].NumberFormat!;
            }

            worksheet.Range(row, 1, row, 2).Style.Border.BottomBorder = XLBorderStyleValues.Hair;
            worksheet.Range(row, 1, row, 2).Style.Border.BottomBorderColor = LightBorderColor;
        }
    }

    private static void ApplyWorksheetDefaults(IXLWorksheet worksheet)
    {
        worksheet.ShowGridLines = false;
        worksheet.Style.Font.FontName = "Arial";
        worksheet.Style.Font.FontSize = 10;
        worksheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    }

    private static void StyleTitle(IXLCell cell)
    {
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 15;
        cell.Style.Font.FontColor = TitleColor;
        cell.WorksheetRow().Height = 24;
    }

    private static void StyleSectionHeader(IXLRange range)
    {
        range.Style.Fill.BackgroundColor = SectionColor;
        range.Style.Font.Bold = true;
        range.Style.Font.FontColor = TitleColor;
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        range.Style.Border.OutsideBorderColor = LightBorderColor;
    }

    private static void StyleTableHeader(IXLRange range)
    {
        range.Style.Fill.BackgroundColor = HeaderColor;
        range.Style.Font.Bold = true;
        range.Style.Font.FontColor = XLColor.White;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        range.Style.Alignment.WrapText = true;
        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        range.Style.Border.InsideBorderColor = XLColor.White;
        range.Worksheet.Rows(DataHeaderRow, DataHeaderRow).Height = 30;
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Clear(XLClearOptions.Contents);
                break;
            case string text:
                cell.Value = text;
                break;
            case int integer:
                cell.Value = integer;
                break;
            case long longInteger:
                cell.Value = longInteger;
                break;
            case decimal decimalValue:
                cell.Value = decimalValue;
                break;
            case double doubleValue:
                cell.Value = doubleValue;
                break;
            case bool boolean:
                cell.Value = boolean;
                break;
            case DateTime dateTime:
                cell.Value = dateTime;
                break;
            case DateOnly dateOnly:
                cell.Value = dateOnly.ToDateTime(TimeOnly.MinValue);
                break;
            default:
                cell.Value = value.ToString() ?? string.Empty;
                break;
        }
    }

    private static object? ToTypedCustomInputValue(BranchTemplatesReportCustomInputValue? value)
    {
        if (value is null)
        {
            return null;
        }

        return value.Type == TemplateCustomInputType.Integer
            ? value.IntegerValue
            : value.StringValue;
    }

    private static object? ResolveLocalizedAnswerValue(
        BranchTemplatesReportAnswer answer,
        bool isArabic)
    {
        return answer.QuestionType switch
        {
            QuestionType.SingleChoice => answer.DisplaySelectedOption(isArabic),
            QuestionType.StarRating => answer.StarRatingValue,
            QuestionType.Smiles => answer.SmileValue,
            QuestionType.Complain => answer.TextAnswer,
            QuestionType.Voice => answer.VoiceFilePath ?? answer.VoiceFileName,
            QuestionType.Image => answer.ImageFilePath ?? answer.ImageFileName,
            _ => answer.DisplayValue
        };
    }

    private static string DisplayTemplateKind(ReportTemplateKind templateKind, bool isArabic)
        => templateKind == ReportTemplateKind.Normal
            ? isArabic ? "مصرح" : "Authorized"
            : isArabic ? "مجهول" : "Anonymous";

    private static decimal? ToExcelPercentage(decimal? percentage)
        => percentage.HasValue ? percentage.Value / 100m : null;

    private static double ResolveColumnWidth(string header)
    {
        if (header.Contains("Question", StringComparison.OrdinalIgnoreCase) ||
            header.Contains("Complaint", StringComparison.OrdinalIgnoreCase) ||
            header.Contains("Trigger", StringComparison.OrdinalIgnoreCase))
        {
            return 36;
        }

        if (header.Contains("URL", StringComparison.OrdinalIgnoreCase) ||
            header.Contains("Path", StringComparison.OrdinalIgnoreCase))
        {
            return 42;
        }

        if (header.Contains("Id", StringComparison.OrdinalIgnoreCase))
        {
            return 38;
        }

        if (header.Contains("Submitted", StringComparison.OrdinalIgnoreCase) ||
            header.Contains("Generated", StringComparison.OrdinalIgnoreCase))
        {
            return 22;
        }

        if (header.Contains("Name", StringComparison.OrdinalIgnoreCase) ||
            header == "Answer")
        {
            return 26;
        }

        return Math.Clamp(header.Length + 3, 12, 24);
    }

    private static string BuildFileName(BranchTemplatesPdfReportModel model)
    {
        var sanitizedTemplateName = InvalidFileNameCharactersRegex()
            .Replace(model.SelectedTemplateName, "-")
            .Trim('-', ' ', '.');
        sanitizedTemplateName = WhitespaceRegex().Replace(sanitizedTemplateName, "-");

        if (string.IsNullOrWhiteSpace(sanitizedTemplateName))
        {
            sanitizedTemplateName = "customer-survey-template-report";
        }

        if (sanitizedTemplateName.Length > 80)
        {
            sanitizedTemplateName = sanitizedTemplateName[..80].TrimEnd('-', ' ', '.');
        }

        return $"{sanitizedTemplateName}-report-{model.GeneratedAtUtc:yyyyMMddTHHmmss}.xlsx";
    }

    private static void EnsureColumnLimit(int columnCount, string sheetName)
    {
        if (columnCount > ExcelMaximumColumns)
        {
            throw new InvalidOperationException(
                $"{sheetName} requires {columnCount} columns, exceeding Excel's {ExcelMaximumColumns}-column limit.");
        }
    }

    [GeneratedRegex(@"[<>:""/\\|?*\x00-\x1F]+")]
    private static partial Regex InvalidFileNameCharactersRegex();

    [GeneratedRegex("\\s+")]
    private static partial Regex WhitespaceRegex();

    private sealed record MatrixCustomInputColumn(
        Guid CustomInputId,
        string Header,
        TemplateCustomInputType Type);

    private sealed record MatrixQuestionColumn(
        Guid TemplateQuestionId,
        string Header);
}

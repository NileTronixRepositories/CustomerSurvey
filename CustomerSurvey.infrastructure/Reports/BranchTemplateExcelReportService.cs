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

        var responseNumbers = BranchTemplatesReportResponseNumberMap.Create(model.Responses);

        AddSummarySheet(workbook, model);
        AddGraphicsSheet(workbook, model);
        AddResponseMatrixSheets(workbook, model, responseNumbers);
        AddResponsesSheets(workbook, model, responseNumbers);
        AddAnswersSheets(workbook, model, responseNumbers);
        AddCustomInputsSheets(workbook, model, responseNumbers);
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

        worksheet.RightToLeft = isArabic;
        worksheet.Cell("A1").Value = T(isArabic, "Customer Survey Template Report", "تقرير نموذج استبيان العملاء");
        StyleTitle(worksheet.Cell("A1"));

        WriteSummarySection(
            worksheet,
            startRow: 3,
            title: T(isArabic, "Applied Filters", "الفلاتر المطبقة"),
            new (string Label, object? Value, string? NumberFormat)[]
            {
                (T(isArabic, "Branch", "الفرع"), model.BranchName, null),
                (T(isArabic, "Template", "النموذج"), template.DisplayName(isArabic), null),
                (T(isArabic, "Template Kind", "نوع النموذج"), template.DisplayKind(isArabic), null),
                (T(isArabic, "From Date", "من تاريخ"), model.FromDate.ToDateTime(TimeOnly.MinValue), "yyyy-mm-dd"),
                (T(isArabic, "To Date", "إلى تاريخ"), model.ToDate.ToDateTime(TimeOnly.MinValue), "yyyy-mm-dd"),
                (T(isArabic, "Score Calculation Mode", "طريقة حساب التقييم"), DisplayScoreMode(model.ScoreCalculationMode, isArabic), null),
                (T(isArabic, "Top Questions Count", "عدد الأسئلة الأعلى"), model.TopWorstQuestionsCount, "0"),
                (T(isArabic, "Worst Questions Threshold", "حد الأسئلة الأسوأ"), model.WorstQuestionsMaxScorePercentage / 100m, "0.00%"),
                (T(isArabic, "Best Questions Threshold", "حد الأسئلة الأفضل"), model.BestQuestionsMinScorePercentage / 100m, "0.00%"),
                (T(isArabic, "Generated By", "تم الإنشاء بواسطة"), model.GeneratedBy, null),
                (T(isArabic, "Generated At", "وقت الإنشاء"), model.GeneratedAtUtc, "yyyy-mm-dd hh:mm:ss")
            });

        var responsesWithScore = model.Responses.Count(x => x.IsScored);

        WriteSummarySection(
            worksheet,
            startRow: 16,
            title: T(isArabic, "Summary Metrics", "مؤشرات الملخص"),
            new (string Label, object? Value, string? NumberFormat)[]
            {
                (T(isArabic, "Total Questions", "إجمالي الأسئلة"), template.TotalQuestions, "0"),
                (T(isArabic, "Root Questions", "الأسئلة الرئيسية"), template.RootQuestions, "0"),
                (T(isArabic, "Conditional Questions", "الأسئلة الشرطية"), template.ConditionalQuestions, "0"),
                (T(isArabic, "Total Responses", "إجمالي الردود"), template.TotalResponses, "0"),
                (T(isArabic, "Total Answers", "إجمالي الإجابات"), template.TotalAnswers, "0"),
                (T(isArabic, "Average Score / 5", "متوسط التقييم / 5"), template.AverageScoreValue, "0.00"),
                (T(isArabic, "Average Satisfaction %", "متوسط الرضا %"), ToExcelPercentage(template.AverageScorePercentage), "0.00%"),
                (T(isArabic, "Responses With Score", "الردود ذات التقييم"), responsesWithScore, "0"),
                (T(isArabic, "Responses Without Score", "الردود بدون تقييم"), Math.Max(0, model.Responses.Count - responsesWithScore), "0")
            });

        var explanationRow = 28;
        worksheet.Cell(explanationRow, 1).Value = T(isArabic, "Scoring Explanation", "شرح حساب التقييم");
        StyleSectionHeader(worksheet.Range(explanationRow, 1, explanationRow, 2));
        worksheet.Cell(explanationRow + 1, 1).Value = isArabic
            ? model.ScoreCalculationMode == ScoreCalculationMode.RootQuestions
                ? "تدخل الأسئلة الرئيسية القابلة للتقييم والمجاب عنها فقط في تقييم كل رد."
                : "يتبع التقييم مسار الشروط المطابق ويستخدم آخر سؤال صالح للتقييم وفق قواعد التقرير الحالية."
            : model.ScoreCalculationMode == ScoreCalculationMode.RootQuestions
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

    private static void AddGraphicsSheet(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model)
    {
        var worksheet = workbook.Worksheets.Add("02 - Graphics");
        ApplyWorksheetDefaults(worksheet);
        worksheet.RightToLeft = model.IsArabic;
        worksheet.Cell("A1").Value = T(model.IsArabic, "Visual Analytics", "التحليلات المرئية");
        StyleTitle(worksheet.Cell("A1"));

        var graphics = model.Graphics;
        var satisfaction = graphics.OverallSatisfactionPercentage;

        AddGraphicsCard(
            worksheet,
            startRow: 3,
            startColumn: 1,
            T(model.IsArabic, "Overall Satisfaction", "الرضا العام"),
            satisfaction.HasValue
                ? new[]
                {
                    new ChartSegment(T(model.IsArabic, "Satisfaction", "الرضا"), satisfaction.Value, "#2563EB"),
                    new ChartSegment(T(model.IsArabic, "Remaining", "المتبقي"), Math.Max(0m, 100m - satisfaction.Value), "#E2E8F0")
                }
                : Array.Empty<ChartSegment>(),
            new[]
            {
                (T(model.IsArabic, "Overall Satisfaction", "الرضا العام"), satisfaction.HasValue ? $"{satisfaction:0.##}%" : T(model.IsArabic, "No Data", "لا توجد بيانات")),
                (T(model.IsArabic, "Average Score", "متوسط التقييم"), graphics.AverageScoreValue.HasValue ? $"{graphics.AverageScoreValue:0.##} / 5" : "—")
            });

        AddGraphicsCard(
            worksheet,
            3,
            10,
            T(model.IsArabic, "Responses Scored vs Not Scored", "الردود المقيمة مقابل غير المقيمة"),
            graphics.TotalResponses > 0
                ? new[]
                {
                    new ChartSegment(T(model.IsArabic, "Scored Responses", "الردود المقيمة"), graphics.ScoredResponses, "#16A34A"),
                    new ChartSegment(T(model.IsArabic, "Not Scored Responses", "الردود غير المقيمة"), graphics.NotScoredResponses, "#94A3B8")
                }
                : Array.Empty<ChartSegment>(),
            new[]
            {
                (T(model.IsArabic, "Scored Responses", "الردود المقيمة"), graphics.ScoredResponses.ToString()),
                (T(model.IsArabic, "Not Scored Responses", "الردود غير المقيمة"), graphics.NotScoredResponses.ToString())
            });

        AddGraphicsCard(
            worksheet,
            21,
            1,
            T(model.IsArabic, "Response Score Distribution", "توزيع تقييم الردود"),
            graphics.ScoreDistributionTotal > 0
                ? new[]
                {
                    new ChartSegment(T(model.IsArabic, "Excellent", "ممتاز"), graphics.ExcellentResponses, "#15803D"),
                    new ChartSegment(T(model.IsArabic, "Good", "جيد"), graphics.GoodResponses, "#65A30D"),
                    new ChartSegment(T(model.IsArabic, "Average", "متوسط"), graphics.AverageResponses, "#EAB308"),
                    new ChartSegment(T(model.IsArabic, "Poor", "ضعيف"), graphics.PoorResponses, "#F97316"),
                    new ChartSegment(T(model.IsArabic, "Critical", "حرج"), graphics.CriticalResponses, "#DC2626")
                }
                : Array.Empty<ChartSegment>(),
            new[]
            {
                (T(model.IsArabic, "Excellent", "ممتاز"), graphics.ExcellentResponses.ToString()),
                (T(model.IsArabic, "Good", "جيد"), graphics.GoodResponses.ToString()),
                (T(model.IsArabic, "Average", "متوسط"), graphics.AverageResponses.ToString()),
                (T(model.IsArabic, "Poor", "ضعيف"), graphics.PoorResponses.ToString()),
                (T(model.IsArabic, "Critical", "حرج"), graphics.CriticalResponses.ToString())
            });

        AddGraphicsCard(
            worksheet,
            21,
            10,
            T(model.IsArabic, "Root vs Conditional Questions", "الأسئلة الرئيسية مقابل الشرطية"),
            graphics.RootQuestions + graphics.ConditionalQuestions > 0
                ? new[]
                {
                    new ChartSegment(T(model.IsArabic, "Root Questions", "الأسئلة الرئيسية"), graphics.RootQuestions, "#2563EB"),
                    new ChartSegment(T(model.IsArabic, "Conditional Questions", "الأسئلة الشرطية"), graphics.ConditionalQuestions, "#F59E0B")
                }
                : Array.Empty<ChartSegment>(),
            new[]
            {
                (T(model.IsArabic, "Root Questions", "الأسئلة الرئيسية"), graphics.RootQuestions.ToString()),
                (T(model.IsArabic, "Conditional Questions", "الأسئلة الشرطية"), graphics.ConditionalQuestions.ToString())
            });

        AddGraphicsCard(
            worksheet,
            39,
            1,
            T(model.IsArabic, "Scored vs Non-Scored Answers", "الإجابات المحتسبة مقابل غير المحتسبة"),
            graphics.IncludedAnswers + graphics.NonScoredAnswers > 0
                ? new[]
                {
                    new ChartSegment(T(model.IsArabic, "Scored Answers", "الإجابات المحتسبة"), graphics.IncludedAnswers, "#7C3AED"),
                    new ChartSegment(T(model.IsArabic, "Non-Scored Answers", "الإجابات غير المحتسبة"), graphics.NonScoredAnswers, "#CBD5E1")
                }
                : Array.Empty<ChartSegment>(),
            new[]
            {
                (T(model.IsArabic, "Scored Answers", "الإجابات المحتسبة"), graphics.IncludedAnswers.ToString()),
                (T(model.IsArabic, "Non-Scored Answers", "الإجابات غير المحتسبة"), graphics.NonScoredAnswers.ToString())
            });

        for (var column = 1; column <= 18; column++)
        {
            worksheet.Column(column).Width = 11;
        }

        worksheet.SheetView.ZoomScale = 85;
    }

    private static void AddResponseMatrixSheets(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model,
        BranchTemplatesReportResponseNumberMap responseNumbers)
    {
        var isArabic = model.IsArabic;
        var customInputColumns = BuildMatrixCustomInputColumns(model);
        var questionColumns = BuildMatrixQuestionColumns(model);

        var headers = new List<string>
        {
            T(isArabic, "Response No", "رقم الرد"),
            T(isArabic, "Submitted At", "وقت الإرسال"),
            T(isArabic, "Operator", "المشغل"),
            T(isArabic, "Scoring Status", "حالة التقييم"),
            T(isArabic, "Score", "التقييم"),
            T(isArabic, "Score %", "نسبة التقييم %")
        };

        headers.AddRange(customInputColumns.Select(x => x.Header));
        headers.AddRange(questionColumns.Select(x => x.Header));
        headers.Add("Survey Response Id");
        headers = MakeHeadersUnique(headers);

        EnsureColumnLimit(headers.Count, "Response Matrix");

        var rows = OrderResponses(model.Responses)
            .Select(response =>
            {
                var values = new List<object?>
                {
                    responseNumbers.GetNumber(response),
                    response.SubmittedOnUtc,
                    response.DisplayOperatorName(isArabic),
                    DisplayScoringStatus(response.IsScored, isArabic),
                    response.IsScored ? response.AverageScoreValue : null,
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

                values.Add(response.ResponseId.ToString());

                return (IReadOnlyList<object?>)values;
            })
            .ToArray();

        const int percentageColumn = 6;
        var wrapColumns = Enumerable.Range(7, Math.Max(0, headers.Count - 7)).ToHashSet();
        var textColumns = customInputColumns
            .Select((column, index) => new { column.Type, ExcelColumn = 7 + index })
            .Where(x => x.Type == TemplateCustomInputType.String)
            .Select(x => x.ExcelColumn)
            .ToHashSet();

        AddDataSheets(
            workbook,
            "03 - Response Matrix",
            "ResponseMatrixTable",
            headers,
            rows,
            freezeColumns: 6,
            percentageColumns: new HashSet<int> { percentageColumn },
            wrapColumns,
            textColumns,
            hiddenColumns: new HashSet<int> { headers.Count },
            dateColumns: new HashSet<int> { 2 },
            isArabic: isArabic);
    }

    private static void AddResponsesSheets(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model,
        BranchTemplatesReportResponseNumberMap responseNumbers)
    {
        var isArabic = model.IsArabic;
        var headers = new[]
        {
            T(isArabic, "Response No", "رقم الرد"),
            T(isArabic, "Submitted At", "وقت الإرسال"),
            T(isArabic, "Operator", "المشغل"),
            T(isArabic, "Scoring Status", "حالة التقييم"),
            T(isArabic, "Score", "التقييم"),
            T(isArabic, "Score %", "نسبة التقييم %"),
            "Survey Response Id"
        };

        var rows = OrderResponses(model.Responses)
            .Select(response => (IReadOnlyList<object?>)new object?[]
            {
                responseNumbers.GetNumber(response),
                response.SubmittedOnUtc,
                response.DisplayOperatorName(isArabic),
                DisplayScoringStatus(response.IsScored, isArabic),
                response.IsScored ? response.AverageScoreValue : null,
                response.IsScored ? ToExcelPercentage(response.ScorePercentage) : null,
                response.ResponseId.ToString()
            })
            .ToArray();

        AddDataSheets(
            workbook,
            "04 - Responses",
            "ResponsesTable",
            headers,
            rows,
            freezeColumns: 3,
            percentageColumns: new HashSet<int> { 6 },
            wrapColumns: new HashSet<int>(),
            hiddenColumns: new HashSet<int> { 7 },
            dateColumns: new HashSet<int> { 2 },
            isArabic: isArabic);
    }

    private static void AddAnswersSheets(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model,
        BranchTemplatesReportResponseNumberMap responseNumbers)
    {
        var isArabic = model.IsArabic;
        var headers = new[]
        {
            T(isArabic, "Response No", "رقم الرد"),
            T(isArabic, "Submitted At", "وقت الإرسال"),
            T(isArabic, "Operator", "المشغل"),
            T(isArabic, "Question", "السؤال"),
            T(isArabic, "Type", "النوع"),
            T(isArabic, "Level", "المستوى"),
            T(isArabic, "Parent Trigger", "شرط الظهور"),
            T(isArabic, "Answer", "الإجابة"),
            T(isArabic, "Option Value", "قيمة الاختيار"),
            T(isArabic, "Score Value", "قيمة التقييم"),
            T(isArabic, "Included In Score", "محتسبة في التقييم"),
            T(isArabic, "Score Inclusion Reason", "سبب الاحتساب"),
            T(isArabic, "Media", "الوسائط"),
            "Survey Response Id"
        };

        var rows = OrderResponses(model.Responses)
            .SelectMany(response => response.Answers.Select(answer => (IReadOnlyList<object?>)new object?[]
            {
                responseNumbers.GetNumber(response),
                response.SubmittedOnUtc,
                response.DisplayOperatorName(isArabic),
                answer.DisplayQuestionText(isArabic),
                DisplayQuestionType(answer.QuestionType, isArabic),
                answer.IsRootQuestion ? (isArabic ? "رئيسي" : "Root") : (isArabic ? "شرطي" : "Conditional"),
                answer.DisplayParentTrigger(isArabic),
                ResolveBusinessAnswerValue(answer, isArabic),
                answer.QuestionType == QuestionType.SingleChoice ? answer.SelectedOptionValue : null,
                answer.ScoreValue,
                DisplayYesNo(answer.IncludedInScore, isArabic),
                DisplayScoreInclusionReason(answer.ScoreInclusionReason, isArabic),
                ResolveMediaValue(answer, isArabic),
                response.ResponseId.ToString()
            }))
            .ToArray();

        AddDataSheets(
            workbook,
            "05 - Answers",
            "AnswersTable",
            headers,
            rows,
            freezeColumns: 3,
            percentageColumns: new HashSet<int>(),
            wrapColumns: new HashSet<int> { 4, 7, 8, 12, 13 },
            hiddenColumns: new HashSet<int> { 14 },
            dateColumns: new HashSet<int> { 2 },
            isArabic: isArabic);
    }

    private static void AddCustomInputsSheets(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model,
        BranchTemplatesReportResponseNumberMap responseNumbers)
    {
        var isArabic = model.IsArabic;
        var headers = new[]
        {
            T(isArabic, "Response No", "رقم الرد"),
            T(isArabic, "Submitted At", "وقت الإرسال"),
            T(isArabic, "Operator", "المشغل"),
            T(isArabic, "Custom Input", "الحقل المخصص"),
            T(isArabic, "Value", "القيمة"),
            "Survey Response Id"
        };

        var rows = OrderResponses(model.Responses)
            .SelectMany(response => response.CustomInputs.Select(customInput => (IReadOnlyList<object?>)new object?[]
            {
                responseNumbers.GetNumber(response),
                response.SubmittedOnUtc,
                response.DisplayOperatorName(isArabic),
                customInput.Name,
                ToTypedCustomInputValue(customInput),
                response.ResponseId.ToString()
            }))
            .ToArray();

        AddDataSheets(
            workbook,
            "06 - Custom Inputs",
            "CustomInputsTable",
            headers,
            rows,
            freezeColumns: 3,
            percentageColumns: new HashSet<int>(),
            wrapColumns: new HashSet<int> { 4, 5 },
            hiddenColumns: new HashSet<int> { 6 },
            dateColumns: new HashSet<int> { 2 },
            isArabic: isArabic);
    }

    private static void AddQuestionAnalysisSheets(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model)
    {
        var isArabic = model.IsArabic;
        var headers = new[]
        {
            "#",
            T(isArabic, "Question", "السؤال"),
            T(isArabic, "Type", "النوع"),
            T(isArabic, "Level", "المستوى"),
            T(isArabic, "Parent Trigger", "شرط الظهور"),
            T(isArabic, "Total Answers", "إجمالي الإجابات"),
            T(isArabic, "Skipped", "تم التخطي"),
            T(isArabic, "Average Score", "متوسط التقييم"),
            T(isArabic, "Satisfaction %", "نسبة الرضا %"),
            T(isArabic, "Included In Score", "محتسبة في التقييم")
        };

        var rows = model.Questions
            .OrderBy(x => x.QuestionOrder)
            .ThenBy(x => x.TemplateQuestionId)
            .Select((question, index) => (IReadOnlyList<object?>)new object?[]
            {
                index + 1,
                question.DisplayQuestionText(isArabic),
                DisplayQuestionType(question.QuestionType, isArabic),
                question.DisplayLevel(isArabic),
                question.DisplayParentTrigger(isArabic),
                question.TotalAnswers,
                question.SkippedCount,
                question.ScoreAverageValue,
                ToExcelPercentage(question.ScoreAveragePercentage),
                DisplayYesNo(question.IsScoreIncluded, isArabic)
            })
            .ToArray();

        AddDataSheets(
            workbook,
            "07 - Question Analysis",
            "QuestionAnalysisTable",
            headers,
            rows,
            freezeColumns: 2,
            percentageColumns: new HashSet<int> { 9 },
            wrapColumns: new HashSet<int> { 2, 5 },
            isArabic: isArabic);
    }

    private static void AddRankingSheets(
        XLWorkbook workbook,
        BranchTemplatesPdfReportModel model,
        bool worst)
    {
        var isArabic = model.IsArabic;
        var headers = new[]
        {
            T(isArabic, "Rank", "الترتيب"),
            T(isArabic, "Question", "السؤال"),
            T(isArabic, "Type", "النوع"),
            T(isArabic, "Level", "المستوى"),
            T(isArabic, "Answers", "الإجابات"),
            T(isArabic, "Average Score", "متوسط التقييم"),
            T(isArabic, "Satisfaction %", "نسبة الرضا %")
        };

        var ranking = worst ? model.WorstQuestions : model.BestQuestions;
        var rows = ranking
            .Select(question => (IReadOnlyList<object?>)new object?[]
            {
                question.Rank,
                question.DisplayQuestionText(isArabic),
                DisplayQuestionType(question.QuestionType, isArabic),
                question.DisplayLevel(isArabic),
                question.TotalAnswers,
                question.AverageScoreValue,
                question.SatisfactionPercentage / 100m
            })
            .ToArray();

        AddDataSheets(
            workbook,
            worst ? "08 - Worst Questions" : "09 - Best Questions",
            worst ? "WorstQuestionsTable" : "BestQuestionsTable",
            headers,
            rows,
            freezeColumns: 1,
            percentageColumns: new HashSet<int> { 7 },
            wrapColumns: new HashSet<int> { 2 },
            isArabic: isArabic);
    }

    private static void AddGraphicsCard(
        IXLWorksheet worksheet,
        int startRow,
        int startColumn,
        string title,
        IReadOnlyList<ChartSegment> segments,
        IReadOnlyList<(string Label, string Value)> details)
    {
        var titleRange = worksheet.Range(startRow, startColumn, startRow, startColumn + 7);
        titleRange.Merge();
        titleRange.FirstCell().Value = title;
        StyleSectionHeader(titleRange);
        titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var image = DonutChartPngRenderer.Render(
            segments.Select(x => (x.Value, x.Color)).ToArray(),
            width: 220,
            height: 180);
        using var imageStream = new MemoryStream(image);
        worksheet.AddPicture(imageStream, ClosedXML.Excel.Drawings.XLPictureFormat.Png, $"Chart-{startRow}-{startColumn}")
            .MoveTo(worksheet.Cell(startRow + 2, startColumn))
            .WithSize(220, 180);

        var detailRow = startRow + 3;
        if (segments.Count == 0)
        {
            worksheet.Cell(detailRow, startColumn + 5).Value = worksheet.RightToLeft ? "لا توجد بيانات" : "No Data";
            worksheet.Cell(detailRow, startColumn + 5).Style.Font.Bold = true;
            detailRow++;
        }

        foreach (var detail in details)
        {
            var matchingSegment = segments.FirstOrDefault(segment =>
                string.Equals(segment.Label, detail.Label, StringComparison.Ordinal));

            if (matchingSegment is not null)
            {
                worksheet.Cell(detailRow, startColumn + 4).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(matchingSegment.Color);
            }

            worksheet.Cell(detailRow, startColumn + 5).Value = detail.Label;
            worksheet.Cell(detailRow, startColumn + 5).Style.Font.Bold = true;
            worksheet.Cell(detailRow, startColumn + 7).Value = detail.Value;
            detailRow++;
        }

        worksheet.Range(startRow + 1, startColumn, startRow + 16, startColumn + 7)
            .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        worksheet.Range(startRow + 1, startColumn, startRow + 16, startColumn + 7)
            .Style.Border.OutsideBorderColor = LightBorderColor;
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
        IReadOnlySet<int>? textColumns = null,
        IReadOnlySet<int>? hiddenColumns = null,
        IReadOnlySet<int>? dateColumns = null,
        bool isArabic = false)
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
            worksheet.RightToLeft = isArabic;

            worksheet.Cell(1, 1).Value = DisplaySheetTitle(baseSheetName, isArabic);
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
                var columnWidth = ResolveColumnWidth(headers[column - 1]);
                if (dateColumns?.Contains(column) == true)
                {
                    columnWidth = Math.Max(columnWidth, 22);
                }

                worksheet.Column(column).Width = columnWidth;

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

                if (dateColumns?.Contains(column) == true && rowCount > 0)
                {
                    worksheet.Range(DataHeaderRow + 1, column, lastRow, column)
                        .Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
                }

                if (hiddenColumns?.Contains(column) == true)
                {
                    worksheet.Column(column).Hide();
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

            if (header is "Submitted On UTC" or "Submitted At" or "وقت الإرسال")
            {
                range.Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
            }
            else if ((header.Contains("Score", StringComparison.OrdinalIgnoreCase) ||
                      header.Contains("تقييم", StringComparison.OrdinalIgnoreCase)) &&
                      !header.Contains("Percentage", StringComparison.OrdinalIgnoreCase) &&
                      !header.Contains("%", StringComparison.OrdinalIgnoreCase) &&
                      header != "Scoring Status" &&
                      header != "Scored Items Count" &&
                      header != "حالة التقييم")
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
            case ExcelHyperlinkValue hyperlink:
                cell.Value = hyperlink.DisplayText;
                cell.SetHyperlink(new XLHyperlink(hyperlink.Url));
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

    private static object? ResolveBusinessAnswerValue(
        BranchTemplatesReportAnswer answer,
        bool isArabic)
        => answer.QuestionType switch
        {
            QuestionType.Voice => T(isArabic, "Voice attachment", "مرفق صوتي"),
            QuestionType.Image => T(isArabic, "Image attachment", "مرفق صورة"),
            _ => ResolveLocalizedAnswerValue(answer, isArabic)
        };

    private static object? ResolveMediaValue(
        BranchTemplatesReportAnswer answer,
        bool isArabic)
    {
        var path = answer.QuestionType switch
        {
            QuestionType.Voice => answer.VoiceFilePath ?? answer.VoiceFileName,
            QuestionType.Image => answer.ImageFilePath ?? answer.ImageFileName,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (Uri.TryCreate(path, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return new ExcelHyperlinkValue(
                answer.QuestionType == QuestionType.Voice
                    ? T(isArabic, "Open Voice", "فتح الصوت")
                    : T(isArabic, "View Image", "عرض الصورة"),
                path);
        }

        return path;
    }

    private static IReadOnlyCollection<BranchTemplatesReportResponse> OrderResponses(
        IEnumerable<BranchTemplatesReportResponse> responses)
        => responses
            .OrderBy(x => x.SubmittedOnUtc)
            .ThenBy(x => x.ResponseId)
            .ToArray();

    private static string DisplayScoringStatus(bool isScored, bool isArabic)
        => isScored
            ? T(isArabic, "Scored", "مُقيّم")
            : T(isArabic, "Not Scored", "غير مُقيّم");

    private static string DisplayScoreMode(ScoreCalculationMode mode, bool isArabic)
        => mode == ScoreCalculationMode.RootQuestions
            ? T(isArabic, "Root Questions", "الأسئلة الرئيسية")
            : T(isArabic, "Lowest Condition Level", "أدنى مستوى شرطي");

    private static string DisplayQuestionType(QuestionType questionType, bool isArabic)
        => questionType switch
        {
            QuestionType.SingleChoice => T(isArabic, "Single Choice", "اختيار واحد"),
            QuestionType.StarRating => T(isArabic, "Star Rating", "تقييم النجوم"),
            QuestionType.Smiles => T(isArabic, "Smiles", "الوجوه التعبيرية"),
            QuestionType.Complain => T(isArabic, "Complaint", "شكوى"),
            QuestionType.Voice => T(isArabic, "Voice", "صوت"),
            QuestionType.Image => T(isArabic, "Image", "صورة"),
            _ => questionType.ToString()
        };

    private static string DisplayQuestionType(string questionType, bool isArabic)
        => Enum.TryParse<QuestionType>(questionType, out var parsed)
            ? DisplayQuestionType(parsed, isArabic)
            : questionType;

    private static string DisplayScoreInclusionReason(string reason, bool isArabic)
    {
        if (!isArabic)
        {
            return reason;
        }

        return reason switch
        {
            "Non-Scorable Question" => "سؤال غير قابل للتقييم",
            "Root Question" => "سؤال رئيسي",
            "Selected By Lowest Condition Level" => "تم اختياره حسب أدنى مستوى شرطي",
            "Root Question - Not Included" => "سؤال رئيسي - غير محتسب",
            "Conditional Question - Not Included" => "سؤال شرطي - غير محتسب",
            _ => reason
        };
    }

    private static string DisplaySheetTitle(string baseSheetName, bool isArabic)
    {
        if (!isArabic)
        {
            return baseSheetName[(baseSheetName.IndexOf('-') + 1)..].Trim();
        }

        return baseSheetName switch
        {
            "03 - Response Matrix" => "مصفوفة الردود",
            "04 - Responses" => "الردود",
            "05 - Answers" => "الإجابات",
            "06 - Custom Inputs" => "الحقول المخصصة",
            "07 - Question Analysis" => "تحليل الأسئلة",
            "08 - Worst Questions" => "الأسئلة الأسوأ",
            "09 - Best Questions" => "الأسئلة الأفضل",
            _ => baseSheetName[(baseSheetName.IndexOf('-') + 1)..].Trim()
        };
    }

    private static string T(bool isArabic, string english, string arabic)
        => isArabic ? arabic : english;

    private static string DisplayYesNo(bool value, bool isArabic)
        => value
            ? T(isArabic, "Yes", "نعم")
            : T(isArabic, "No", "لا");

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

    private sealed record ChartSegment(
        string Label,
        decimal Value,
        string Color);

    private sealed record ExcelHyperlinkValue(
        string DisplayText,
        string Url);
}

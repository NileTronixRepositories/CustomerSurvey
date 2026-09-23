using CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Tests;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class DashboardDrillDownTests
{
    [Theory]
    [InlineData(59.99, SatisfactionCategory.Unhappy)]
    [InlineData(60, SatisfactionCategory.Neutral)]
    [InlineData(79.99, SatisfactionCategory.Neutral)]
    [InlineData(80, SatisfactionCategory.Satisfied)]
    public void Satisfaction_rule_preserves_existing_boundaries(
        decimal scorePercentage,
        SatisfactionCategory expected)
    {
        Assert.Equal(expected, SatisfactionCategoryRule.Classify(1, scorePercentage));
    }

    [Fact]
    public void Satisfaction_rule_never_categorizes_unscored_response()
    {
        Assert.Null(SatisfactionCategoryRule.Classify(0, 100m));
        Assert.False(SatisfactionCategoryRule.Matches(0, 0m, SatisfactionCategory.Unhappy));
    }

    [Fact]
    public void Distribution_uses_scored_denominator_and_rounds_to_two_decimals()
    {
        var scores = new[] { 80m, 90m, 60m, 59.99m, 10m, 100m };

        var result = SatisfactionDistributionBuilder.Build(
            scores,
            score => score,
            category => DashboardDrillDownPathBuilder.Navigation(
                "BranchResponses",
                "/api/reports/branch-responses",
                ("satisfactionCategory", category)));

        Assert.Collection(
            result,
            satisfied =>
            {
                Assert.Equal(SatisfactionCategory.Satisfied, satisfied.Category);
                Assert.Equal(3, satisfied.ResponsesCount);
                Assert.Equal(50m, satisfied.Percentage);
            },
            neutral =>
            {
                Assert.Equal(SatisfactionCategory.Neutral, neutral.Category);
                Assert.Equal(1, neutral.ResponsesCount);
                Assert.Equal(16.67m, neutral.Percentage);
            },
            unhappy =>
            {
                Assert.Equal(SatisfactionCategory.Unhappy, unhappy.Category);
                Assert.Equal(2, unhappy.ResponsesCount);
                Assert.Equal(33.33m, unhappy.Percentage);
            });
    }

    [Fact]
    public void Empty_distribution_returns_all_categories_with_zero_percentages()
    {
        var result = SatisfactionDistributionBuilder.Build(
            Array.Empty<decimal>(),
            score => score,
            category => DashboardDrillDownPathBuilder.Navigation("Responses", "/responses"));

        Assert.Equal(3, result.Count);
        Assert.All(result, item =>
        {
            Assert.Equal(0, item.ResponsesCount);
            Assert.Equal(0m, item.Percentage);
        });
    }

    [Fact]
    public void Navigation_builder_preserves_context_and_url_encodes_values()
    {
        var templateId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var branchId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var path = DashboardDrillDownPathBuilder.Build(
            "/api/reports/branch-responses",
            ("from", new DateOnly(2026, 9, 1)),
            ("to", new DateOnly(2026, 9, 30)),
            ("templateId", templateId),
            ("branchId", branchId),
            ("source", SurveyDashboardSource.All),
            ("scoreCalculationMode", ScoreCalculationMode.RootQuestions),
            ("customInputName", "Region / المنطقة"),
            ("customInputValue", "North & East"),
            ("pageNumber", 1),
            ("pageSize", 10));

        Assert.StartsWith("/api/reports/branch-responses?", path);
        Assert.Contains("from=2026-09-01", path);
        Assert.Contains("to=2026-09-30", path);
        Assert.Contains($"templateId={templateId}", path);
        Assert.Contains($"branchId={branchId}", path);
        Assert.Contains("source=All", path);
        Assert.Contains("scoreCalculationMode=RootQuestions", path);
        Assert.Contains("customInputName=Region%20%2F%20", path);
        Assert.Contains("customInputValue=North%20%26%20East", path);
    }

    [Fact]
    public void Monthly_trend_range_is_clipped_to_applied_dashboard_period()
    {
        var range = DashboardDrillDownPathBuilder.ResolveTrendRange(
            "2026-09",
            true,
            new DateOnly(2026, 9, 15),
            new DateOnly(2026, 9, 30));

        Assert.Equal(new DateOnly(2026, 9, 15), range.From);
        Assert.Equal(new DateOnly(2026, 9, 30), range.To);
    }

    [Fact]
    public void Branch_response_spec_combines_satisfaction_question_and_custom_input_filters()
    {
        var questionId = Guid.NewGuid();
        var response = SurveyResponse.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            actualScore: 3,
            maxScore: 5,
            scorePercentage: 60m);
        response.AddAnswer(SurveyAnswer.CreateStarRating(response.Id, questionId, 3));
        response.AddCustomInputValue(SurveyResponseCustomInputValue.CreateStringValue(
            response.Id,
            Guid.NewGuid(),
            "Region",
            "North"));

        var query = new GetBranchSurveyResponsesPaginationQuery
        {
            SatisfactionCategory = SatisfactionCategory.Neutral,
            QuestionId = questionId,
            CustomInputName = "Region",
            CustomInputType = TemplateCustomInputType.String,
            CustomInputValue = "North"
        };
        var spec = new GetBranchSurveyResponsesPaginationSpec(
            branchId: null,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            query);

        Assert.True(spec.Criteria.Compile()(response));

        var wrongQuestion = new GetBranchSurveyResponsesPaginationSpec(
            branchId: null,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            new GetBranchSurveyResponsesPaginationQuery
            {
                SatisfactionCategory = SatisfactionCategory.Neutral,
                QuestionId = Guid.NewGuid(),
                CustomInputName = "Region",
                CustomInputType = TemplateCustomInputType.String,
                CustomInputValue = "North"
            });

        Assert.False(wrongQuestion.Criteria.Compile()(response));
    }

    [Fact]
    public void Department_response_spec_enforces_department_scope_and_filters()
    {
        var departmentId = Guid.NewGuid();
        var operatorProfile = Operator.Create(Guid.NewGuid(), departmentId, Guid.NewGuid());
        var questionId = Guid.NewGuid();
        var response = SurveyResponse.Create(
            operatorProfile.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            actualScore: 2,
            maxScore: 5,
            scorePercentage: 40m);
        SetProperty(response, nameof(SurveyResponse.Operator), operatorProfile);
        response.AddAnswer(SurveyAnswer.CreateStarRating(response.Id, questionId, 2));
        response.AddCustomInputValue(SurveyResponseCustomInputValue.CreateIntegerValue(
            response.Id,
            Guid.NewGuid(),
            "Age",
            30));

        var query = new GetDepartmentSurveyResponsesPaginationQuery
        {
            SatisfactionCategory = SatisfactionCategory.Unhappy,
            QuestionId = questionId,
            CustomInputName = "Age",
            CustomInputType = TemplateCustomInputType.Integer,
            CustomInputValue = "30"
        };

        var matching = new GetDepartmentSurveyResponsesPaginationSpec(
            departmentId,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            query);
        var otherDepartment = new GetDepartmentSurveyResponsesPaginationSpec(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            query);

        Assert.True(matching.Criteria.Compile()(response));
        Assert.False(otherDepartment.Criteria.Compile()(response));
        Assert.True(matching.IsPagingEnabled);
    }

    [Fact]
    public async Task Department_response_handler_resolves_department_from_authenticated_admin()
    {
        var currentUserId = Guid.NewGuid();
        var department = Department.Create("Support", null, currentUserId);
        var otherDepartment = Department.Create("Sales", null, currentUserId);
        var departmentAdmin = DepartmentAdmin.Create(currentUserId, department.Id, currentUserId);
        SetProperty(departmentAdmin, nameof(DepartmentAdmin.Department), department);

        var ownResponse = CreateDepartmentResponse(department, currentUserId, 55m);
        var otherResponse = CreateDepartmentResponse(otherDepartment, currentUserId, 55m);

        var adminRepository = new InMemoryReadRepository<DepartmentAdmin>();
        adminRepository.Add(departmentAdmin);
        var responseRepository = new InMemoryReadRepository<SurveyResponse>();
        responseRepository.AddRange(new[] { ownResponse, otherResponse });
        var customInputRepository = new InMemoryReadRepository<SurveyResponseCustomInputValue>();

        var handler = new GetDepartmentSurveyResponsesPaginationQueryHandler(
            adminRepository,
            responseRepository,
            customInputRepository,
            new TestCurrentUser(currentUserId));

        var result = await handler.Handle(
            new GetDepartmentSurveyResponsesPaginationQuery
            {
                SatisfactionCategory = SatisfactionCategory.Unhappy,
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Data);
        Assert.Equal(ownResponse.Id, result.Value.Data[0].SurveyResponseId);
    }

    private static SurveyResponse CreateDepartmentResponse(
        Department department,
        Guid createdBy,
        decimal scorePercentage)
    {
        var applicationUser = ApplicationUser.Create(
            $"operator-{Guid.NewGuid():N}",
            $"{Guid.NewGuid():N}@example.com",
            "Operator",
            null,
            null,
            "hash",
            UserType.Operator,
            createdBy);
        var operatorProfile = Operator.Create(applicationUser.Id, department.Id, createdBy);
        SetProperty(operatorProfile, nameof(Operator.ApplicationUser), applicationUser);
        SetProperty(operatorProfile, nameof(Operator.Department), department);

        var branch = Branch.Create("Branch", null, Guid.NewGuid().ToString("N"), null, createdBy);
        var template = Template.Create(
            branch.Id,
            "Template",
            null,
            null,
            DateTime.UtcNow.AddDays(-1),
            null,
            createdBy);
        SetProperty(template, nameof(Template.Branch), branch);

        var response = SurveyResponse.Create(
            operatorProfile.Id,
            template.Id,
            createdBy,
            actualScore: 2,
            maxScore: 5,
            scorePercentage);
        SetProperty(response, nameof(SurveyResponse.Operator), operatorProfile);
        SetProperty(response, nameof(SurveyResponse.Template), template);
        return response;
    }

    private static void SetProperty<T>(T instance, string propertyName, object value)
    {
        typeof(T).GetProperty(propertyName)!.SetValue(instance, value);
    }
}

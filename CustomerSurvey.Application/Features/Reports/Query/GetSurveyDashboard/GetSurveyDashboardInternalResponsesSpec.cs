using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

internal sealed class GetSurveyDashboardInternalResponsesSpec
    : Specification<SurveyResponse, SurveyDashboardResponseRow>
{
    public GetSurveyDashboardInternalResponsesSpec(
        Guid? branchId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? templateId)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.SubmittedOnUtc >= fromUtc &&
            x.SubmittedOnUtc < toExclusiveUtc);

        if (branchId.HasValue)
        {
            AddCriteria(x => x.Template.BranchId == branchId.Value);
        }

        if (templateId.HasValue)
        {
            AddCriteria(x => x.TemplateId == templateId.Value);
        }

        AddOrderByDescending(x => x.SubmittedOnUtc);

        Select(x => new SurveyDashboardResponseRow
        {
            Source = SurveyDashboardSource.Internal,
            ResponseId = x.Id,
            TemplateId = x.TemplateId,
            TemplateNameEn = x.Template.NameEn,
            TemplateNameAr = x.Template.NameAr,
            BranchId = x.Template.BranchId,
            BranchNameEn = x.Template.Branch.NameEn,
            BranchNameAr = x.Template.Branch.NameAr,
            OperatorId = x.OperatorId,
            OperatorNameEn = x.Operator.ApplicationUser.NameEn,
            OperatorNameAr = x.Operator.ApplicationUser.NameAr,
            SubmittedOnUtc = x.SubmittedOnUtc,
            MaxScore = x.MaxScore,
            ScorePercentage = x.ScorePercentage,
            HasComplaint = x.Answers.Any(answer =>
                answer.QuestionType == QuestionType.Complain &&
                answer.TextAnswer != null &&
                answer.TextAnswer != string.Empty),
            HasVoice = x.Answers.Any(answer =>
                answer.QuestionType == QuestionType.Voice &&
                answer.VoiceFileName != null &&
                answer.VoiceFileName != string.Empty)
        });
    }
}

internal sealed class GetSurveyDashboardInternalAnswersSpec
    : Specification<SurveyAnswer, SurveyDashboardAnswerRow>
{
    public GetSurveyDashboardInternalAnswersSpec(
        Guid? branchId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? templateId)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.SurveyResponse.SubmittedOnUtc >= fromUtc &&
            x.SurveyResponse.SubmittedOnUtc < toExclusiveUtc);

        if (branchId.HasValue)
        {
            AddCriteria(x => x.SurveyResponse.Template.BranchId == branchId.Value);
        }

        if (templateId.HasValue)
        {
            AddCriteria(x => x.SurveyResponse.TemplateId == templateId.Value);
        }

        Select(x => new SurveyDashboardAnswerRow
        {
            Source = SurveyDashboardSource.Internal,
            ResponseId = x.SurveyResponseId,
            TemplateId = x.SurveyResponse.TemplateId,
            TemplateNameEn = x.SurveyResponse.Template.NameEn,
            TemplateNameAr = x.SurveyResponse.Template.NameAr,
            BranchId = x.SurveyResponse.Template.BranchId,
            BranchNameEn = x.SurveyResponse.Template.Branch.NameEn,
            BranchNameAr = x.SurveyResponse.Template.Branch.NameAr,
            QuestionId = x.QuestionId,
            QuestionTextEn = x.Question.TextEn,
            QuestionTextAr = x.Question.TextAr,
            QuestionType = x.QuestionType,
            StarRatingValue = x.StarRatingValue,
            SmileValue = x.SmileValue,
            SelectedQuestionOptionValue = x.SelectedQuestionOption == null
                ? null
                : x.SelectedQuestionOption.Value,
            TextAnswer = x.TextAnswer,
            VoiceFileName = x.VoiceFileName
        });
    }
}

internal sealed class GetSurveyDashboardInternalCustomInputValuesSpec
    : Specification<SurveyResponseCustomInputValue, SurveyDashboardCustomInputValueRow>
{
    public GetSurveyDashboardInternalCustomInputValuesSpec(
        Guid? branchId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? templateId)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.SurveyResponse.SubmittedOnUtc >= fromUtc &&
            x.SurveyResponse.SubmittedOnUtc < toExclusiveUtc);

        if (branchId.HasValue)
        {
            AddCriteria(x => x.SurveyResponse.Template.BranchId == branchId.Value);
        }

        if (templateId.HasValue)
        {
            AddCriteria(x => x.SurveyResponse.TemplateId == templateId.Value);
        }

        Select(x => new SurveyDashboardCustomInputValueRow
        {
            Source = SurveyDashboardSource.Internal,
            ResponseId = x.SurveyResponseId,
            BranchId = x.SurveyResponse.Template.BranchId,
            NameSnapshot = x.NameSnapshot,
            LabelEn = x.TemplateCustomInput.LabelEn,
            LabelAr = x.TemplateCustomInput.LabelAr,
            TypeSnapshot = x.TypeSnapshot,
            StringValue = x.StringValue,
            IntegerValue = x.IntegerValue,
            ScorePercentage = x.SurveyResponse.ScorePercentage,
            MaxScore = x.SurveyResponse.MaxScore
        });
    }
}

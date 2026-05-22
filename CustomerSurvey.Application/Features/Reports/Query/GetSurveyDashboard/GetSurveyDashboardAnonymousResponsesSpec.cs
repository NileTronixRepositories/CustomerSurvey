using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

internal sealed class GetSurveyDashboardAnonymousResponsesSpec
    : Specification<AnonymousSurveyResponse, SurveyDashboardResponseRow>
{
    public GetSurveyDashboardAnonymousResponsesSpec(
        Guid? branchId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? anonymousTemplateId)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.AnonymousTemplate.Scope == AnonymousTemplateScope.Branch &&
            x.AnonymousTemplate.BranchId != null &&
            x.SubmittedOnUtc >= fromUtc &&
            x.SubmittedOnUtc < toExclusiveUtc);

        if (branchId.HasValue)
        {
            AddCriteria(x => x.AnonymousTemplate.BranchId == branchId.Value);
        }

        if (anonymousTemplateId.HasValue)
        {
            AddCriteria(x => x.AnonymousTemplateId == anonymousTemplateId.Value);
        }

        AddOrderByDescending(x => x.SubmittedOnUtc);

        Select(x => new SurveyDashboardResponseRow
        {
            Source = SurveyDashboardSource.Anonymous,
            ResponseId = x.Id,
            TemplateId = x.AnonymousTemplateId,
            TemplateNameEn = x.AnonymousTemplate.NameEn,
            TemplateNameAr = x.AnonymousTemplate.NameAr,
            BranchId = x.AnonymousTemplate.BranchId!.Value,
            BranchNameEn = x.AnonymousTemplate.Branch!.NameEn,
            BranchNameAr = x.AnonymousTemplate.Branch.NameAr,
            OperatorId = null,
            OperatorNameEn = null,
            OperatorNameAr = null,
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

internal sealed class GetSurveyDashboardAnonymousAnswersSpec
    : Specification<AnonymousSurveyAnswer, SurveyDashboardAnswerRow>
{
    public GetSurveyDashboardAnonymousAnswersSpec(
        Guid? branchId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? anonymousTemplateId)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.AnonymousSurveyResponse.AnonymousTemplate.Scope == AnonymousTemplateScope.Branch &&
            x.AnonymousSurveyResponse.AnonymousTemplate.BranchId != null &&
            x.AnonymousSurveyResponse.SubmittedOnUtc >= fromUtc &&
            x.AnonymousSurveyResponse.SubmittedOnUtc < toExclusiveUtc);

        if (branchId.HasValue)
        {
            AddCriteria(x => x.AnonymousSurveyResponse.AnonymousTemplate.BranchId == branchId.Value);
        }

        if (anonymousTemplateId.HasValue)
        {
            AddCriteria(x => x.AnonymousSurveyResponse.AnonymousTemplateId == anonymousTemplateId.Value);
        }

        Select(x => new SurveyDashboardAnswerRow
        {
            Source = SurveyDashboardSource.Anonymous,
            ResponseId = x.AnonymousSurveyResponseId,
            TemplateId = x.AnonymousSurveyResponse.AnonymousTemplateId,
            TemplateNameEn = x.AnonymousSurveyResponse.AnonymousTemplate.NameEn,
            TemplateNameAr = x.AnonymousSurveyResponse.AnonymousTemplate.NameAr,
            BranchId = x.AnonymousSurveyResponse.AnonymousTemplate.BranchId!.Value,
            BranchNameEn = x.AnonymousSurveyResponse.AnonymousTemplate.Branch!.NameEn,
            BranchNameAr = x.AnonymousSurveyResponse.AnonymousTemplate.Branch.NameAr,
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

internal sealed class GetSurveyDashboardAnonymousCustomInputValuesSpec
    : Specification<AnonymousSurveyResponseCustomInputValue, SurveyDashboardCustomInputValueRow>
{
    public GetSurveyDashboardAnonymousCustomInputValuesSpec(
        Guid? branchId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? anonymousTemplateId)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.AnonymousSurveyResponse.AnonymousTemplate.Scope == AnonymousTemplateScope.Branch &&
            x.AnonymousSurveyResponse.AnonymousTemplate.BranchId != null &&
            x.AnonymousSurveyResponse.SubmittedOnUtc >= fromUtc &&
            x.AnonymousSurveyResponse.SubmittedOnUtc < toExclusiveUtc);

        if (branchId.HasValue)
        {
            AddCriteria(x => x.AnonymousSurveyResponse.AnonymousTemplate.BranchId == branchId.Value);
        }

        if (anonymousTemplateId.HasValue)
        {
            AddCriteria(x => x.AnonymousSurveyResponse.AnonymousTemplateId == anonymousTemplateId.Value);
        }

        Select(x => new SurveyDashboardCustomInputValueRow
        {
            Source = SurveyDashboardSource.Anonymous,
            ResponseId = x.AnonymousSurveyResponseId,
            BranchId = x.AnonymousSurveyResponse.AnonymousTemplate.BranchId!.Value,
            NameSnapshot = x.NameSnapshot,
            LabelEn = x.AnonymousTemplateCustomInput.LabelEn,
            LabelAr = x.AnonymousTemplateCustomInput.LabelAr,
            TypeSnapshot = x.TypeSnapshot,
            StringValue = x.StringValue,
            IntegerValue = x.IntegerValue,
            ScorePercentage = x.AnonymousSurveyResponse.ScorePercentage,
            MaxScore = x.AnonymousSurveyResponse.MaxScore
        });
    }
}

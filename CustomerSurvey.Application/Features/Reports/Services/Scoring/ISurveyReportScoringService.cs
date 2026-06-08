using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

namespace CustomerSurvey.Application.Features.Reports.Services.Scoring;

public interface ISurveyReportScoringService
{
    IReadOnlyCollection<CalculatedResponseScore> CalculateResponseScores(
        IReadOnlyCollection<ResponseFlatDto> responses,
        IReadOnlyCollection<AnswerFlatDto> answers,
        IReadOnlyCollection<TemplateQuestionFlatDto> templateQuestions,
        IReadOnlyCollection<ConditionFlatDto> conditions,
        IReadOnlyCollection<QuestionOptionFlatDto> questionOptions,
        ScoreCalculationMode scoreCalculationMode);

    IReadOnlyCollection<QuestionScoreToken> CalculateQuestionScoreTokens(
        IReadOnlyCollection<ResponseFlatDto> responses,
        IReadOnlyCollection<AnswerFlatDto> answers,
        IReadOnlyCollection<TemplateQuestionFlatDto> templateQuestions,
        IReadOnlyCollection<ConditionFlatDto> conditions,
        IReadOnlyCollection<QuestionOptionFlatDto> questionOptions,
        ScoreCalculationMode scoreCalculationMode);
}

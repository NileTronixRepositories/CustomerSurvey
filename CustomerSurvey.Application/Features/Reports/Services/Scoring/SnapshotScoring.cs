using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System.Text.Json;

namespace CustomerSurvey.Application.Features.Reports.Services.Scoring;

public static class SnapshotScoring
{
    public static ResponseSnapshotDocument? Read(string? payload)
        => string.IsNullOrWhiteSpace(payload) ? null : JsonSerializer.Deserialize<ResponseSnapshotDocument>(payload);

    public static IReadOnlyList<QuestionScoreToken> Tokens(ResponseSnapshotDocument snapshot, ScoreCalculationMode mode)
    {
        if (!snapshot.IsHistoricallyVerified) return Array.Empty<QuestionScoreToken>();
        var included = SnapshotScoreSelection.Questions(snapshot, (int)mode);
        return included.Select(q => new QuestionScoreToken
        {
            ResponseId = snapshot.ResponseId, TemplateId = snapshot.TemplateId,
            TemplateKind = (ReportTemplateKind)snapshot.TemplateKind,
            TemplateQuestionId = q.TemplateQuestionId, QuestionId = q.QuestionId,
            ScoreValue = Value(q)!.Value, ScorePercentage = Value(q)!.Value * 20m,
            HistoricalQuestion = q
        }).ToArray();
    }

    public static int? Value(SnapshotQuestion q) => SnapshotScoreSelection.Value(q);

    public static IReadOnlyCollection<TemplateQuestionFlatDto> HistoricalQuestions(
        IEnumerable<ResponseFlatDto> responses, IEnumerable<TemplateQuestionFlatDto> current, bool preserveGroupHistory = false)
    {
        var documents = responses.OrderByDescending(x => x.SubmittedOnUtc)
            .Select(x => Read(x.SnapshotPayload)).Where(x => x is not null).ToArray();
        var documentedTemplates = documents.Select(x => ((ReportTemplateKind)x!.TemplateKind, x.TemplateId)).ToHashSet();
        var historical = documents.SelectMany(s => s!.Questions.Select(q => new TemplateQuestionFlatDto
            {
                TemplateKind = (ReportTemplateKind)s.TemplateKind, TemplateId = s.TemplateId,
                IsRootAtSubmission = q.IsRoot, TemplateQuestionId = q.TemplateQuestionId, QuestionId = q.QuestionId, QuestionType = q.Type,
                Order = q.Order, QuestionTextEn = q.TextEn, QuestionTextAr = q.TextAr,
                QuestionGroupId = q.GroupId, QuestionGroupNameEn = q.GroupNameEn, QuestionGroupNameAr = q.GroupNameAr
            }));
        var definitions = historical.Concat(current.Where(x => !documentedTemplates.Contains((x.TemplateKind, x.TemplateId)))).Where(x => x.TemplateQuestionId != Guid.Empty)
            .GroupBy(x => (x.TemplateKind, x.TemplateId, x.TemplateQuestionId, GroupId: preserveGroupHistory ? x.QuestionGroupId : Guid.Empty))
            .Select(x => x.First()).ToArray();
        var labels = definitions.GroupBy(x => (x.TemplateKind, x.TemplateId, x.QuestionGroupId)).ToDictionary(x => x.Key, x => x.First());
        return definitions.Select(x => x with
        {
            QuestionGroupNameEn = labels[(x.TemplateKind, x.TemplateId, x.QuestionGroupId)].QuestionGroupNameEn,
            QuestionGroupNameAr = labels[(x.TemplateKind, x.TemplateId, x.QuestionGroupId)].QuestionGroupNameAr
        }).ToArray();
    }

    public static IReadOnlyCollection<ConditionFlatDto> HistoricalConditions(IEnumerable<ResponseFlatDto> responses, IEnumerable<ConditionFlatDto> current)
    {
        var documents = responses.Select(x => Read(x.SnapshotPayload)).Where(x => x is not null).ToArray();
        var templates = documents.Select(x => ((ReportTemplateKind)x!.TemplateKind, x.TemplateId)).ToHashSet();
        return documents.SelectMany(s => s!.Conditions.Select(c => new ConditionFlatDto
        {
            TemplateKind = (ReportTemplateKind)s.TemplateKind, TemplateId = s.TemplateId,
            ParentTemplateQuestionId = c.ParentTemplateQuestionId, ChildTemplateQuestionId = c.ChildTemplateQuestionId,
            TriggerType = c.TriggerType, SelectedQuestionOptionId = c.SelectedQuestionOptionId, TriggerValue = c.TriggerValue
        })).Concat(current.Where(x => !templates.Contains((x.TemplateKind, x.TemplateId))))
            .Distinct().ToArray();
    }

    public static IReadOnlyCollection<AnswerFlatDto> HistoricalAnswers(
        IEnumerable<ResponseFlatDto> responses, IEnumerable<AnswerFlatDto> current)
    {
        var saved = responses.Select(x => Read(x.SnapshotPayload)).Where(x => x is not null)
            .ToDictionary(x => ((ReportTemplateKind)x!.TemplateKind, x.ResponseId));
        return current.Select(a =>
        {
            var q = saved.GetValueOrDefault((a.TemplateKind, a.ResponseId))?.Questions.FirstOrDefault(x => x.QuestionId == a.QuestionId);
            return q is null ? a : a with
            {
                HistoricalQuestion = q, TemplateQuestionId = q.TemplateQuestionId,
                QuestionType = q.Type, QuestionTextEn = q.TextEn, QuestionTextAr = q.TextAr,
                SelectedQuestionOptionId = q.SelectedQuestionOptionId, StarRatingValue = q.StarRatingValue,
                SmileValue = q.SmileValue, TextAnswer = q.TextAnswer, VoiceFileName = q.VoiceFileName,
                ImageFileName = q.ImageFileName
            };
        }).ToArray();
    }
}

using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.infrastructure.Persistence;

internal static class ResponseSnapshotCapture
{
    internal static async Task<ResponseSnapshotDocument> BuildAsync(PlatformWriteDbContext db,
        SurveyResponse? response, AnonymousSurveyResponse? anonymous, bool legacy, CancellationToken ct)
    {
        var kind = response is not null ? 1 : 2;
        var templateId = response?.TemplateId ?? anonymous!.AnonymousTemplateId;
        var answers = response is not null
            ? response.Answers.Select(a => new Answer(a.QuestionId, a.QuestionType, a.SelectedQuestionOptionId,
                a.StarRatingValue, a.SmileValue, a.TextAnswer, a.VoiceFileName, a.ImageFileName)).ToArray()
            : anonymous!.Answers.Select(a => new Answer(a.QuestionId, a.QuestionType, a.SelectedQuestionOptionId,
                a.StarRatingValue, a.SmileValue, a.TextAnswer, a.VoiceFileName, a.ImageFileName)).ToArray();

        var questions = kind == 1
            ? await db.Set<TemplateQuestion>().AsNoTracking().Where(x => x.TemplateId == templateId)
                .Select(x => new Definition(x.Id, x.QuestionId, x.Order, x.Question)).ToListAsync(ct)
            : await db.Set<AnonymousTemplateQuestion>().AsNoTracking().Where(x => x.AnonymousTemplateId == templateId)
                .Select(x => new Definition(x.Id, x.QuestionId, x.Order, x.Question)).ToListAsync(ct);
        var ids = questions.Select(x => x.QuestionId).ToArray();
        var groups = await db.Set<Question>().AsNoTracking().Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, x.GroupId, x.Group.NameEn, x.Group.NameAr }).ToDictionaryAsync(x => x.Id, ct);
        var options = await db.Set<QuestionOption>().AsNoTracking().Where(x => ids.Contains(x.QuestionId))
            .ToListAsync(ct);
        var conditions = kind == 1
            ? await db.Set<TemplateQuestionCondition>().AsNoTracking().Where(x => x.TemplateId == templateId && x.IsActive)
                .Select(x => new SnapshotCondition(x.ParentTemplateQuestionId, x.ChildTemplateQuestionId,
                    x.TriggerType, x.SelectedQuestionOptionId, x.TriggerValue, x.Order)).ToListAsync(ct)
            : await db.Set<AnonymousTemplateQuestionCondition>().AsNoTracking().Where(x => x.AnonymousTemplateId == templateId && x.IsActive)
                .Select(x => new SnapshotCondition(x.ParentAnonymousTemplateQuestionId, x.ChildAnonymousTemplateQuestionId,
                    x.TriggerType, x.SelectedQuestionOptionId, x.TriggerValue, x.Order)).ToListAsync(ct);
        var children = conditions.Select(x => x.ChildTemplateQuestionId).ToHashSet();
        var included = new List<SnapshotQuestion>();
        foreach (var answer in answers)
        {
            var definition = questions.FirstOrDefault(x => x.QuestionId == answer.QuestionId);
            if (definition is null && !legacy)
                throw new InvalidOperationException("A submitted question is missing from its template snapshot.");
            var selected = options.FirstOrDefault(x => x.Id == answer.OptionId);
            var group = groups.GetValueOrDefault(answer.QuestionId);
            included.Add(new SnapshotQuestion
            {
                TemplateQuestionId = definition?.Id ?? Guid.Empty, QuestionId = answer.QuestionId,
                Type = answer.Type, TextEn = definition?.Question.TextEn ?? string.Empty,
                TextAr = definition?.Question.TextAr, Order = definition?.Order ?? 0,
                GroupId = group?.GroupId ?? Guid.Empty, GroupNameEn = group?.NameEn ?? string.Empty,
                GroupNameAr = group?.NameAr, IsRoot = definition is not null && !children.Contains(definition.Id),
                HasAnswer = true, SelectedQuestionOptionId = answer.OptionId, SelectedOptionValue = selected?.Value,
                SelectedOptionTextEn = selected?.TextEn, SelectedOptionTextAr = selected?.TextAr,
                StarRatingValue = answer.Star, SmileValue = answer.Smile, TextAnswer = answer.Text,
                VoiceFileName = answer.Voice, ImageFileName = answer.Image,
                Options = options.Where(x => x.QuestionId == answer.QuestionId && (x.IsActive || x.Id == answer.OptionId))
                    .OrderBy(x => x.Order).Select(x => new SnapshotOption(x.Id, x.TextEn, x.TextAr, x.Value, x.Order, x.IsActive)).ToArray()
            });
        }
        var includedIds = included.Select(x => x.TemplateQuestionId).ToHashSet();
        var customInputs = kind == 1
            ? await db.Set<TemplateCustomInput>().AsNoTracking().Where(x => x.TemplateId == templateId && x.IsActive)
                .Select(x => new SnapshotCustomInputDefinition(x.Id, x.LabelEn, x.LabelAr, x.Type, x.IsRequired,
                    x.MinLength, x.MaxLength, x.MinValue, x.MaxValue, x.StartWith, x.Order)).ToListAsync(ct)
            : await db.Set<AnonymousTemplateCustomInput>().AsNoTracking().Where(x => x.AnonymousTemplateId == templateId && x.IsActive)
                .Select(x => new SnapshotCustomInputDefinition(x.Id, x.LabelEn, x.LabelAr, x.Type, x.IsRequired,
                    x.MinLength, x.MaxLength, x.MinValue, x.MaxValue, x.StartWith, x.Order)).ToListAsync(ct);
        var template = kind == 1
            ? await db.Set<Template>().Where(x => x.Id == templateId).Select(x => new { BranchId = (Guid?)x.BranchId, x.NameEn, x.NameAr }).FirstAsync(ct)
            : await db.Set<AnonymousTemplate>().Where(x => x.Id == templateId).Select(x => new { x.BranchId, x.NameEn, x.NameAr }).FirstAsync(ct);
        return new ResponseSnapshotDocument
        {
            ResponseId = response?.Id ?? anonymous!.Id, TemplateId = templateId, TemplateKind = kind,
            BranchId = template.BranchId, TemplateNameEn = template.NameEn, TemplateNameAr = template.NameAr, OperatorId = response?.OperatorId,
            SubmittedOnUtc = response?.SubmittedOnUtc ?? anonymous!.SubmittedOnUtc,
            ActualScore = response?.ActualScore ?? anonymous!.ActualScore,
            MaxScore = response?.MaxScore ?? anonymous!.MaxScore,
            ScorePercentage = response?.ScorePercentage ?? anonymous!.ScorePercentage,
            Provenance = legacy ? SnapshotProvenance.LegacyIncomplete : SnapshotProvenance.CapturedAtSubmission,
            Questions = included.OrderBy(x => x.Order).ToArray(),
            Conditions = conditions.Where(x => includedIds.Contains(x.ParentTemplateQuestionId) &&
                includedIds.Contains(x.ChildTemplateQuestionId)).ToArray(), CustomInputDefinitions = customInputs
        };
    }

    private sealed record Definition(Guid Id, Guid QuestionId, int Order, Question Question);
    private sealed record Answer(Guid QuestionId, CustomerSurvey.Domain.Enums.QuestionType Type,
        Guid? OptionId, int? Star, int? Smile, string? Text, string? Voice, string? Image);
}

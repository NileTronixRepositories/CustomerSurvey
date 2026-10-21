using CustomerSurvey.Domain.Enums;
namespace CustomerSurvey.Domain.Entities;

public static class SnapshotScoreSelection
{
    public static IReadOnlyList<SnapshotQuestion> Questions(ResponseSnapshotDocument snapshot, int mode)
    {
        if (!snapshot.IsHistoricallyVerified) return Array.Empty<SnapshotQuestion>();
        var questions = snapshot.Questions.ToDictionary(x => x.TemplateQuestionId);
        var included = new List<SnapshotQuestion>();
        if (mode == 3)
            included.AddRange(snapshot.Questions.Where(x => x.HasAnswer && Value(x).HasValue));
        else foreach (var root in snapshot.Questions.Where(x => x.IsRoot))
        {
            var current = root;
            SnapshotQuestion? last = null;
            var visited = new HashSet<Guid>();
            while (current.HasAnswer && visited.Add(current.TemplateQuestionId))
            {
                if (Value(current).HasValue) last = current;
                if (mode == 1) break;
                var condition = snapshot.Conditions.Where(x => x.ParentTemplateQuestionId == current.TemplateQuestionId)
                    .OrderBy(x => questions.GetValueOrDefault(x.ChildTemplateQuestionId)?.Order ?? int.MaxValue)
                    .FirstOrDefault(x => Matches(x, current));
                if (condition is null || !questions.TryGetValue(condition.ChildTemplateQuestionId, out var child)) break;
                current = child;
            }
            if (last is not null) included.Add(last);
        }
        return included;
    }

    public static int? Value(SnapshotQuestion q)
    {
        var value = q.Type switch
        {
            QuestionType.SingleChoice => q.SelectedOptionValue,
            QuestionType.StarRating => q.StarRatingValue,
            QuestionType.Smiles => q.SmileValue,
            _ => null
        };
        return value is >= 1 and <= 5 ? value : null;
    }

    private static bool Matches(SnapshotCondition c, SnapshotQuestion q) => c.TriggerType switch
    {
        QuestionConditionTriggerType.SingleChoiceOption => q.SelectedQuestionOptionId.HasValue && q.SelectedQuestionOptionId == c.SelectedQuestionOptionId,
        QuestionConditionTriggerType.StarRatingValue => q.StarRatingValue.HasValue && q.StarRatingValue == c.TriggerValue,
        QuestionConditionTriggerType.SmileValue => q.SmileValue.HasValue && q.SmileValue == c.TriggerValue,
        _ => false
    };
}

using BuildingBlock.Domain.EntitiesHelper;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class AnonymousSurveyResponse : AggregateRoot<Guid>
    {
        private readonly List<AnonymousSurveyAnswer> _answers = new();
        private readonly List<AnonymousSurveyResponseCustomInputValue> _customInputValues = new();

        public Guid AnonymousTemplateId { get; private set; }
        public AnonymousTemplate AnonymousTemplate { get; private set; } = null!;

        public ResponseSnapshot? Snapshot { get; private set; }

        public void CaptureSnapshot(ResponseSnapshotDocument document)
        {
            if (Snapshot is not null) throw new InvalidOperationException("Response snapshots are immutable.");
            if (document.ResponseId != Id || document.TemplateKind != 2 || document.TemplateId != AnonymousTemplateId ||
                document.ActualScore != ActualScore || document.MaxScore != MaxScore || document.ScorePercentage != ScorePercentage) throw new ArgumentException("Snapshot response identity mismatch.");
            Snapshot = ResponseSnapshot.Create(document);
        }

        public DateTime SubmittedOnUtc { get; private set; }

        public int ActualScore { get; private set; }

        public int MaxScore { get; private set; }

        public decimal ScorePercentage { get; private set; }

        public IReadOnlyCollection<AnonymousSurveyAnswer> Answers =>
            _answers.AsReadOnly();

        public IReadOnlyCollection<AnonymousSurveyResponseCustomInputValue> CustomInputValues =>
            _customInputValues.AsReadOnly();

        private AnonymousSurveyResponse()
        {
        }

        public static AnonymousSurveyResponse Create(
            Guid anonymousTemplateId,
            int actualScore,
            int maxScore,
            decimal scorePercentage)
        {
            return new AnonymousSurveyResponse
            {
                Id = Guid.NewGuid(),
                AnonymousTemplateId = anonymousTemplateId,
                SubmittedOnUtc = DateTime.UtcNow,
                ActualScore = actualScore,
                MaxScore = maxScore,
                ScorePercentage = scorePercentage
            };
        }

        public void AddAnswer(AnonymousSurveyAnswer answer)
        {
            if (_answers.Any(x => x.AnonymousTemplateQuestionId == answer.AnonymousTemplateQuestionId))
            {
                return;
            }

            _answers.Add(answer);
        }

        public void AddCustomInputValue(AnonymousSurveyResponseCustomInputValue customInputValue)
        {
            if (_customInputValues.Any(x => x.AnonymousTemplateCustomInputId == customInputValue.AnonymousTemplateCustomInputId))
            {
                return;
            }

            _customInputValues.Add(customInputValue);
        }
    }
}
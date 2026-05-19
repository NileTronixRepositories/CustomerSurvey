using BuildingBlock.Domain.EntitiesHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class SurveyResponse : AggregateRoot<Guid>
    {
        private readonly List<SurveyAnswer> _answers = new();
        private readonly List<SurveyResponseCustomInputValue> _customInputValues = new();

        public Guid OperatorId { get; private set; }
        public CustomerSurvey.Domain.Identity.Operator Operator { get; private set; } = null!;

        public Guid TemplateId { get; private set; }
        public Template Template { get; private set; } = null!;

        public DateTime SubmittedOnUtc { get; private set; }

        public int ActualScore { get; private set; }

        public int MaxScore { get; private set; }

        public decimal ScorePercentage { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<SurveyAnswer> Answers => _answers.AsReadOnly();

        public IReadOnlyCollection<SurveyResponseCustomInputValue> CustomInputValues =>
            _customInputValues.AsReadOnly();

        private SurveyResponse()
        {
        }

        public static SurveyResponse Create(
            Guid operatorId,
            Guid templateId,
            Guid createdByApplicationUserId,
            int actualScore,
            int maxScore,
            decimal scorePercentage)
        {
            return new SurveyResponse
            {
                Id = Guid.NewGuid(),
                OperatorId = operatorId,
                TemplateId = templateId,
                SubmittedOnUtc = DateTime.UtcNow,
                ActualScore = actualScore,
                MaxScore = maxScore,
                ScorePercentage = scorePercentage,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public void AddAnswer(SurveyAnswer answer)
        {
            if (_answers.Any(x => x.QuestionId == answer.QuestionId))
            {
                return;
            }

            _answers.Add(answer);
        }

        public void AddCustomInputValue(SurveyResponseCustomInputValue customInputValue)
        {
            if (_customInputValues.Any(x => x.TemplateCustomInputId == customInputValue.TemplateCustomInputId))
            {
                return;
            }

            _customInputValues.Add(customInputValue);
        }
    }
}
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

        public Guid OperatorId { get; private set; }
        public CustomerSurvey.Domain.Identity.Operator Operator { get; private set; } = null!;

        public Guid TemplateId { get; private set; }
        public Template Template { get; private set; } = null!;

        public DateTime SubmittedOnUtc { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<SurveyAnswer> Answers => _answers.AsReadOnly();

        private SurveyResponse()
        {
        }

        public static SurveyResponse Create(
            Guid operatorId,
            Guid templateId,
            Guid createdByApplicationUserId)
        {
            return new SurveyResponse
            {
                Id = Guid.NewGuid(),
                OperatorId = operatorId,
                TemplateId = templateId,
                SubmittedOnUtc = DateTime.UtcNow,
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
    }
}
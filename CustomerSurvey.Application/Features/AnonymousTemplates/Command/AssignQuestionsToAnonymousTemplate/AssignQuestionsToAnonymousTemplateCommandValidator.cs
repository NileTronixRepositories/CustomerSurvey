using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignQuestionsToAnonymousTemplate
{
    internal sealed class AssignQuestionsToAnonymousTemplateCommandValidator
        : AbstractValidator<AssignQuestionsToAnonymousTemplateCommand>
    {
        public AssignQuestionsToAnonymousTemplateCommandValidator()
        {
            RuleFor(x => x.AnonymousTemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.AssignQuestionsToAnonymousTemplate_AnonymousTemplateId_Required);

            RuleFor(x => x.Questions)
                .NotNull()
                .WithMessage(ErrorMessage.AssignQuestionsToAnonymousTemplate_Questions_Invalid);

            RuleForEach(x => x.Questions)
                .ChildRules(question =>
                {
                    question.RuleFor(x => x.QuestionId)
                        .NotEmpty()
                        .WithMessage(ErrorMessage.AssignQuestionsToAnonymousTemplate_QuestionId_Required);

                    question.RuleFor(x => x.Order)
                        .GreaterThan(0)
                        .WithMessage(ErrorMessage.AssignQuestionsToAnonymousTemplate_Order_Invalid);
                });

            RuleFor(x => x.Questions)
                .Must(HaveUniqueQuestionIds)
                .WithMessage(ErrorMessage.AssignQuestionsToAnonymousTemplate_Question_Duplicated);

            RuleFor(x => x.Questions)
                .Must(HaveUniqueOrders)
                .WithMessage(ErrorMessage.AssignQuestionsToAnonymousTemplate_Order_Duplicated);
        }

        private static bool HaveUniqueQuestionIds(
            IReadOnlyCollection<AssignQuestionToAnonymousTemplateCommandItem> questions)
        {
            var ids = questions
                .Where(x => x.QuestionId != Guid.Empty)
                .Select(x => x.QuestionId)
                .ToArray();

            return ids.Length == ids.Distinct().Count();
        }

        private static bool HaveUniqueOrders(
            IReadOnlyCollection<AssignQuestionToAnonymousTemplateCommandItem> questions)
        {
            var orders = questions
                .Where(x => x.Order > 0)
                .Select(x => x.Order)
                .ToArray();

            return orders.Length == orders.Distinct().Count();
        }
    }
}
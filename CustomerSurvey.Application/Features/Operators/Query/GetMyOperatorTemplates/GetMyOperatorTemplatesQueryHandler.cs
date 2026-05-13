using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Application.Features.Questions.Shared.Specs;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    using DomainOperator = Domain.Identity.Operator;

    internal sealed class GetMyOperatorTemplatesQueryHandler
        : IQueryHandler<GetMyOperatorTemplatesQuery, GetMyOperatorTemplatesResponse>
    {
        private readonly IWriteReadRepository<DomainOperator> _operatorReadRepository;
        private readonly IWriteReadRepository<OperatorTemplate> _operatorTemplateReadRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetMyOperatorTemplatesQueryHandler(
            IWriteReadRepository<DomainOperator> operatorReadRepository,
            IWriteReadRepository<OperatorTemplate> operatorTemplateReadRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            ICurrentUser currentUser)
        {
            _operatorReadRepository = operatorReadRepository
                ?? throw new ArgumentNullException(nameof(operatorReadRepository));

            _operatorTemplateReadRepository = operatorTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(operatorTemplateReadRepository));

            _templateQuestionReadRepository = templateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(templateQuestionReadRepository));

            _questionOptionReadRepository = questionOptionReadRepository
                ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<GetMyOperatorTemplatesResponse>> Handle(
            GetMyOperatorTemplatesQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<GetMyOperatorTemplatesResponse>.Fail(new Error(
                    Code: "Operators.MyTemplates.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentOperator = await _operatorReadRepository.FirstOrDefaultAsync(
                new GetCurrentOperatorForMyTemplatesSpec(currentApplicationUserId),
                cancellationToken);

            if (currentOperator is null)
            {
                return Result<GetMyOperatorTemplatesResponse>.Fail(new Error(
                    Code: "Operators.MyTemplates.CurrentOperatorNotFound",
                    Message: ErrorMessage.GetMyOperatorTemplates_CurrentOperator_NotFound,
                    Type: ErrorType.NotFound));
            }

            var templates = await _operatorTemplateReadRepository.ListAsync(
                new GetAssignedTemplatesForMyOperatorSpec(currentOperator.OperatorId),
                cancellationToken);

            var templateIds = templates
                .Select(x => x.TemplateId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<TemplateQuestionForMyOperatorDto> templateQuestions;

            if (templateIds.Length == 0)
            {
                templateQuestions = Array.Empty<TemplateQuestionForMyOperatorDto>();
            }
            else
            {
                templateQuestions = await _templateQuestionReadRepository.ListAsync(
                    new GetTemplateQuestionsForMyOperatorTemplatesSpec(templateIds),
                    cancellationToken);
            }

            var questionIds = templateQuestions
                .Select(x => x.QuestionId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<QuestionOptionResponse> questionOptions;

            if (questionIds.Length == 0)
            {
                questionOptions = Array.Empty<QuestionOptionResponse>();
            }
            else
            {
                questionOptions = await _questionOptionReadRepository.ListAsync(
                    new GetQuestionOptionsByQuestionIdsSpec(questionIds),
                    cancellationToken);
            }

            var optionsByQuestionId = questionOptions
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<MyOperatorQuestionOptionResponse>)x
                        .OrderBy(option => option.Order)
                        .Select(option => new MyOperatorQuestionOptionResponse
                        {
                            OptionId = option.OptionId,
                            TextEn = option.TextEn,
                            TextAr = option.TextAr,
                            Order = option.Order
                        })
                        .ToArray());

            var questionsByTemplateId = templateQuestions
                .GroupBy(x => x.TemplateId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<MyOperatorTemplateQuestionResponse>)x
                        .OrderBy(q => q.Order)
                        .Select(q =>
                        {
                            IReadOnlyCollection<MyOperatorQuestionOptionResponse> options =
                                q.Type == QuestionType.SingleChoice.ToString()
                                && optionsByQuestionId.TryGetValue(q.QuestionId, out var questionOptionsList)
                                    ? questionOptionsList
                                    : Array.Empty<MyOperatorQuestionOptionResponse>();

                            return new MyOperatorTemplateQuestionResponse
                            {
                                TemplateQuestionId = q.TemplateQuestionId,
                                QuestionId = q.QuestionId,
                                Order = q.Order,
                                TextEn = q.TextEn,
                                TextAr = q.TextAr,
                                Type = q.Type,
                                GroupId = q.GroupId,
                                GroupNameEn = q.GroupNameEn,
                                GroupNameAr = q.GroupNameAr,
                                Options = options
                            };
                        })
                        .ToArray());

            var templateItems = templates
                .Select(template =>
                {
                    IReadOnlyCollection<MyOperatorTemplateQuestionResponse> questions =
                        questionsByTemplateId.TryGetValue(
                            template.TemplateId,
                            out var templateQuestionsList)
                                ? templateQuestionsList
                                : Array.Empty<MyOperatorTemplateQuestionResponse>();

                    return new MyOperatorTemplateItemResponse
                    {
                        TemplateId = template.TemplateId,
                        NameEn = template.NameEn,
                        NameAr = template.NameAr,
                        Description = template.Description,
                        BranchId = template.BranchId,
                        BranchNameEn = template.BranchNameEn,
                        BranchNameAr = template.BranchNameAr,
                        BranchCode = template.BranchCode,
                        QuestionsCount = questions.Count,
                        Questions = questions
                    };
                })
                .ToArray();

            var response = new GetMyOperatorTemplatesResponse
            {
                OperatorId = currentOperator.OperatorId,
                DepartmentId = currentOperator.DepartmentId,
                TemplatesCount = templateItems.Length,
                Templates = templateItems
            };

            return Result<GetMyOperatorTemplatesResponse>.Ok(response);
        }
    }
}
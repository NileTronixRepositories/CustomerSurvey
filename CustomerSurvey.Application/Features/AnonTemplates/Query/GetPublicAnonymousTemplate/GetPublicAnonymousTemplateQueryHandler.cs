using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.AnonTemplates.Query.GetPublicAnonymousTemplate
{
    internal sealed class GetPublicAnonymousTemplateQueryHandler
        : IQueryHandler<GetPublicAnonymousTemplateQuery, GetPublicAnonymousTemplateResponse>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateCustomInput> _customInputReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestionCondition> _conditionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;

        public GetPublicAnonymousTemplateQueryHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteReadRepository<AnonymousTemplateCustomInput> customInputReadRepository,
            IWriteReadRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionReadRepository,
            IWriteReadRepository<AnonymousTemplateQuestionCondition> conditionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository)
        {
            _anonymousTemplateReadRepository = anonymousTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

            _customInputReadRepository = customInputReadRepository
                ?? throw new ArgumentNullException(nameof(customInputReadRepository));

            _anonymousTemplateQuestionReadRepository = anonymousTemplateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateQuestionReadRepository));

            _conditionReadRepository = conditionReadRepository
                ?? throw new ArgumentNullException(nameof(conditionReadRepository));

            _questionOptionReadRepository = questionOptionReadRepository
                ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));
        }

        public async Task<Result<GetPublicAnonymousTemplateResponse>> Handle(
            GetPublicAnonymousTemplateQuery request,
            CancellationToken cancellationToken)
        {
            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetPublicAnonymousTemplateBasicSpec(request.AnonymousTemplateId),
                cancellationToken);

            if (anonymousTemplate is null)
            {
                return Result<GetPublicAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonTemplates.GetPublic.TemplateNotFound",
                    Message: ErrorMessage.GetPublicAnonymousTemplate_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            var utcNow = DateTime.UtcNow;

            if (anonymousTemplate.Scope != AnonymousTemplateScope.Branch ||
                !anonymousTemplate.BranchId.HasValue)
            {
                return Result<GetPublicAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonTemplates.GetPublic.GlobalTemplateNotAvailable",
                    Message: ErrorMessage.GetPublicAnonymousTemplate_Template_NotAvailable,
                    Type: ErrorType.NotFound));
            }

            if (!anonymousTemplate.IsActive)
            {
                return Result<GetPublicAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonTemplates.GetPublic.TemplateInactive",
                    Message: ErrorMessage.GetPublicAnonymousTemplate_Template_NotAvailable,
                    Type: ErrorType.NotFound));
            }

            if (anonymousTemplate.ActiveFrom > utcNow)
            {
                return Result<GetPublicAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonTemplates.GetPublic.TemplateNotStarted",
                    Message: ErrorMessage.GetPublicAnonymousTemplate_Template_NotStarted,
                    Type: ErrorType.Validation));
            }

            if (anonymousTemplate.ExpireTo.HasValue &&
                anonymousTemplate.ExpireTo.Value <= utcNow)
            {
                return Result<GetPublicAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonTemplates.GetPublic.TemplateExpired",
                    Message: ErrorMessage.GetPublicAnonymousTemplate_Template_Expired,
                    Type: ErrorType.Validation));
            }

            var customInputs = await _customInputReadRepository.ListAsync(
                new GetPublicAnonymousTemplateCustomInputsSpec(anonymousTemplate.AnonymousTemplateId),
                cancellationToken);

            var questions = await _anonymousTemplateQuestionReadRepository.ListAsync(
                new GetPublicAnonymousTemplateQuestionsSpec(anonymousTemplate.AnonymousTemplateId),
                cancellationToken);

            if (questions.Count == 0)
            {
                return Result<GetPublicAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonTemplates.GetPublic.TemplateHasNoQuestions",
                    Message: ErrorMessage.GetPublicAnonymousTemplate_Template_HasNoQuestions,
                    Type: ErrorType.Validation));
            }

            var singleChoiceQuestionIds = questions
                .Where(x => x.Type == QuestionType.SingleChoice)
                .Select(x => x.QuestionId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<PublicAnonymousTemplateQuestionOptionResponse> options;

            if (singleChoiceQuestionIds.Length == 0)
            {
                options = Array.Empty<PublicAnonymousTemplateQuestionOptionResponse>();
            }
            else
            {
                options = await _questionOptionReadRepository.ListAsync(
                    new GetPublicAnonymousTemplateQuestionOptionsSpec(singleChoiceQuestionIds),
                    cancellationToken);
            }

            var optionsByQuestionId = options
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<PublicAnonymousTemplateQuestionOptionResponse>)x
                        .OrderBy(option => option.Order)
                        .ToArray());

            var anonymousTemplateQuestionIds = questions
                .Select(x => x.AnonymousTemplateQuestionId)
                .Distinct()
                .ToArray();

            var conditions = await _conditionReadRepository.ListAsync(
                new GetPublicAnonymousTemplateConditionsSpec(
                    anonymousTemplate.AnonymousTemplateId,
                    anonymousTemplateQuestionIds),
                cancellationToken);

            var childQuestionIds = conditions
                .Select(x => x.ChildAnonymousTemplateQuestionId)
                .Distinct()
                .ToHashSet();

            var rootQuestionIds = anonymousTemplateQuestionIds
                .Where(id => !childQuestionIds.Contains(id))
                .ToArray();

            var rootQuestionIdSet = rootQuestionIds.ToHashSet();

            var questionsWithOptionsAndRootFlag = questions
                .OrderBy(x => x.Order)
                .Select(question =>
                {
                    optionsByQuestionId.TryGetValue(
                        question.QuestionId,
                        out var questionOptions);

                    return question with
                    {
                        IsRoot = rootQuestionIdSet.Contains(question.AnonymousTemplateQuestionId),
                        Options = question.Type == QuestionType.SingleChoice
                            ? questionOptions ?? Array.Empty<PublicAnonymousTemplateQuestionOptionResponse>()
                            : Array.Empty<PublicAnonymousTemplateQuestionOptionResponse>()
                    };
                })
                .ToArray();

            var response = new GetPublicAnonymousTemplateResponse
            {
                AnonymousTemplateId = anonymousTemplate.AnonymousTemplateId,
                BranchId = anonymousTemplate.BranchId,
                Scope = anonymousTemplate.Scope,
                ScopeName = anonymousTemplate.Scope.ToString(),
                IsGlobal = anonymousTemplate.Scope == AnonymousTemplateScope.Global,
                NameEn = anonymousTemplate.NameEn,
                NameAr = anonymousTemplate.NameAr,
                Description = anonymousTemplate.Description,
                ActiveFrom = anonymousTemplate.ActiveFrom,
                ExpireTo = anonymousTemplate.ExpireTo,
                LogoPath = anonymousTemplate.LogoPath,
                Branch = anonymousTemplate.BranchId.HasValue
                    ? new PublicAnonymousTemplateBranchResponse
                    {
                        BranchId = anonymousTemplate.BranchId.Value,
                        NameEn = anonymousTemplate.BranchNameEn!,
                        NameAr = anonymousTemplate.BranchNameAr
                    }
                    : null,
                CustomInputs = customInputs
                    .OrderBy(x => x.Order)
                    .ToArray(),
                Questions = questionsWithOptionsAndRootFlag,
                QuestionConditions = conditions
                    .OrderBy(x => x.Order)
                    .ToArray(),
                RootAnonymousTemplateQuestionIds = rootQuestionIds
            };

            return Result<GetPublicAnonymousTemplateResponse>.Ok(response);
        }
    }
}

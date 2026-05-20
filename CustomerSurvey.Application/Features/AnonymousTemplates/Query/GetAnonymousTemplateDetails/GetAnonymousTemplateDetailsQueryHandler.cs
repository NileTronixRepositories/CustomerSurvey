using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDetails
{
    internal sealed class GetAnonymousTemplateDetailsQueryHandler
        : IQueryHandler<GetAnonymousTemplateDetailsQuery, GetAnonymousTemplateDetailsResponse>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateCustomInput> _customInputReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestionCondition> _conditionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetAnonymousTemplateDetailsQueryHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteReadRepository<AnonymousTemplateCustomInput> customInputReadRepository,
            IWriteReadRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionReadRepository,
            IWriteReadRepository<AnonymousTemplateQuestionCondition> conditionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser)
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

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<GetAnonymousTemplateDetailsResponse>> Handle(
            GetAnonymousTemplateDetailsQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<GetAnonymousTemplateDetailsResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Details.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var isSuperAdmin = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            Guid? currentBranchId = null;

            if (!isSuperAdmin)
            {
                var branchActor = await ResolveBranchActorAsync(
                    currentApplicationUserId,
                    cancellationToken);

                if (branchActor is null)
                {
                    return Result<GetAnonymousTemplateDetailsResponse>.Fail(new Error(
                        Code: "AnonymousTemplates.Details.CurrentActorNotFound",
                        Message: ErrorMessage.GetAnonymousTemplateDetails_CurrentActor_NotFound,
                        Type: ErrorType.Security));
                }

                currentBranchId = branchActor.BranchId;
            }

            var basicDetails = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateBasicDetailsSpec(
                    request.AnonymousTemplateId,
                    isSuperAdmin,
                    currentBranchId),
                cancellationToken);

            if (basicDetails is null)
            {
                return Result<GetAnonymousTemplateDetailsResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Details.TemplateNotFound",
                    Message: ErrorMessage.GetAnonymousTemplateDetails_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            var customInputs = await _customInputReadRepository.ListAsync(
                new GetAnonymousTemplateCustomInputsForDetailsSpec(request.AnonymousTemplateId),
                cancellationToken);

            var questions = await _anonymousTemplateQuestionReadRepository.ListAsync(
                new GetAnonymousTemplateQuestionsForDetailsSpec(request.AnonymousTemplateId),
                cancellationToken);

            var questionIds = questions
                .Where(x => x.Type == QuestionType.SingleChoice)
                .Select(x => x.QuestionId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<AnonymousTemplateDetailsQuestionOptionResponse> options;

            if (questionIds.Length == 0)
            {
                options = Array.Empty<AnonymousTemplateDetailsQuestionOptionResponse>();
            }
            else
            {
                options = await _questionOptionReadRepository.ListAsync(
                    new GetAnonymousTemplateQuestionOptionsForDetailsSpec(questionIds),
                    cancellationToken);
            }

            var optionsByQuestionId = options
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<AnonymousTemplateDetailsQuestionOptionResponse>)x
                        .OrderBy(option => option.Order)
                        .ToArray());

            var questionsWithOptions = questions
                .OrderBy(x => x.Order)
                .Select(question =>
                {
                    optionsByQuestionId.TryGetValue(
                        question.QuestionId,
                        out var questionOptions);

                    return question with
                    {
                        Options = question.Type == QuestionType.SingleChoice
                            ? questionOptions ?? Array.Empty<AnonymousTemplateDetailsQuestionOptionResponse>()
                            : Array.Empty<AnonymousTemplateDetailsQuestionOptionResponse>()
                    };
                })
                .ToArray();

            var selectedAnonymousTemplateQuestionIds = questionsWithOptions
                .Select(x => x.AnonymousTemplateQuestionId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<AnonymousTemplateDetailsQuestionConditionResponse> conditions;

            if (selectedAnonymousTemplateQuestionIds.Length == 0)
            {
                conditions = Array.Empty<AnonymousTemplateDetailsQuestionConditionResponse>();
            }
            else
            {
                conditions = await _conditionReadRepository.ListAsync(
                    new GetAnonymousTemplateQuestionConditionsForDetailsSpec(
                        request.AnonymousTemplateId,
                        selectedAnonymousTemplateQuestionIds),
                    cancellationToken);
            }

            var response = basicDetails with
            {
                Summary = new AnonymousTemplateDetailsSummaryResponse
                {
                    QuestionsCount = questionsWithOptions.Length,
                    CustomInputsCount = customInputs.Count,
                    QuestionConditionsCount = conditions.Count
                },
                CustomInputs = customInputs
                    .OrderBy(x => x.Order)
                    .ToArray(),
                Questions = questionsWithOptions,
                QuestionConditions = conditions
                    .OrderBy(x => x.Order)
                    .ToArray()
            };

            return Result<GetAnonymousTemplateDetailsResponse>.Ok(response);
        }

        private async Task<CurrentBranchActorForGetAnonymousTemplateDetailsDto?> ResolveBranchActorAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var currentBranchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForGetAnonymousTemplateDetailsSpec(applicationUserId),
                cancellationToken);

            if (currentBranchAdmin is not null)
            {
                return currentBranchAdmin;
            }

            var currentBranchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForGetAnonymousTemplateDetailsSpec(applicationUserId),
                cancellationToken);

            return currentBranchUser;
        }
    }
}
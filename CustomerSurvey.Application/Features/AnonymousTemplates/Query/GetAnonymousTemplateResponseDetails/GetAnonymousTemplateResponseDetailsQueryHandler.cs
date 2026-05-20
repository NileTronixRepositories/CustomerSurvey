using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    internal sealed class GetAnonymousTemplateResponseDetailsQueryHandler
        : IQueryHandler<GetAnonymousTemplateResponseDetailsQuery, GetAnonymousTemplateResponseDetailsResponse>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteReadRepository<AnonymousSurveyResponse> _anonymousSurveyResponseReadRepository;
        private readonly IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> _customInputValueReadRepository;
        private readonly IWriteReadRepository<AnonymousSurveyAnswer> _answerReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetAnonymousTemplateResponseDetailsQueryHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteReadRepository<AnonymousSurveyResponse> anonymousSurveyResponseReadRepository,
            IWriteReadRepository<AnonymousSurveyResponseCustomInputValue> customInputValueReadRepository,
            IWriteReadRepository<AnonymousSurveyAnswer> answerReadRepository,
            IWriteReadRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser)
        {
            _anonymousTemplateReadRepository = anonymousTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

            _anonymousSurveyResponseReadRepository = anonymousSurveyResponseReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousSurveyResponseReadRepository));

            _customInputValueReadRepository = customInputValueReadRepository
                ?? throw new ArgumentNullException(nameof(customInputValueReadRepository));

            _answerReadRepository = answerReadRepository
                ?? throw new ArgumentNullException(nameof(answerReadRepository));

            _anonymousTemplateQuestionReadRepository = anonymousTemplateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateQuestionReadRepository));

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

        public async Task<Result<GetAnonymousTemplateResponseDetailsResponse>> Handle(
            GetAnonymousTemplateResponseDetailsQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<GetAnonymousTemplateResponseDetailsResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.ResponseDetails.Unauthenticated",
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
                var currentBranchActor = await ResolveBranchActorAsync(
                    currentApplicationUserId,
                    cancellationToken);

                if (currentBranchActor is null)
                {
                    return Result<GetAnonymousTemplateResponseDetailsResponse>.Fail(new Error(
                        Code: "AnonymousTemplates.ResponseDetails.CurrentActorNotFound",
                        Message: ErrorMessage.GetAnonymousTemplateResponseDetails_CurrentActor_NotFound,
                        Type: ErrorType.Security));
                }

                currentBranchId = currentBranchActor.BranchId;
            }

            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForResponseDetailsSpec(
                    request.AnonymousTemplateId,
                    isSuperAdmin,
                    currentBranchId),
                cancellationToken);

            if (anonymousTemplate is null)
            {
                return Result<GetAnonymousTemplateResponseDetailsResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.ResponseDetails.TemplateNotFound",
                    Message: ErrorMessage.GetAnonymousTemplateResponseDetails_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            var responseBasic = await _anonymousSurveyResponseReadRepository.FirstOrDefaultAsync(
                new GetAnonymousSurveyResponseBasicDetailsSpec(
                    request.AnonymousTemplateId,
                    request.ResponseId),
                cancellationToken);

            if (responseBasic is null)
            {
                return Result<GetAnonymousTemplateResponseDetailsResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.ResponseDetails.ResponseNotFound",
                    Message: ErrorMessage.GetAnonymousTemplateResponseDetails_Response_NotFound,
                    Type: ErrorType.NotFound));
            }

            var customInputValues = await _customInputValueReadRepository.ListAsync(
                new GetAnonymousSurveyResponseCustomInputValuesDetailsSpec(request.ResponseId),
                cancellationToken);

            var answers = await _answerReadRepository.ListAsync(
                new GetAnonymousSurveyResponseAnswersDetailsSpec(request.ResponseId),
                cancellationToken);

            var anonymousTemplateQuestionIds = answers
                .Select(x => x.AnonymousTemplateQuestionId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<AnonymousTemplateQuestionForResponseDetailsDto> templateQuestions;

            if (anonymousTemplateQuestionIds.Length == 0)
            {
                templateQuestions = Array.Empty<AnonymousTemplateQuestionForResponseDetailsDto>();
            }
            else
            {
                templateQuestions = await _anonymousTemplateQuestionReadRepository.ListAsync(
                    new GetAnonymousTemplateQuestionsForResponseDetailsSpec(
                        request.AnonymousTemplateId,
                        anonymousTemplateQuestionIds),
                    cancellationToken);
            }

            var questionsByAnonymousTemplateQuestionId = templateQuestions
                .ToDictionary(x => x.AnonymousTemplateQuestionId);

            var selectedOptionIds = answers
                .Where(x => x.SelectedQuestionOptionId.HasValue)
                .Select(x => x.SelectedQuestionOptionId!.Value)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<QuestionOptionForResponseDetailsDto> selectedOptions;

            if (selectedOptionIds.Length == 0)
            {
                selectedOptions = Array.Empty<QuestionOptionForResponseDetailsDto>();
            }
            else
            {
                selectedOptions = await _questionOptionReadRepository.ListAsync(
                    new GetQuestionOptionsForResponseDetailsSpec(selectedOptionIds),
                    cancellationToken);
            }

            var selectedOptionsById = selectedOptions.ToDictionary(x => x.OptionId);

            var hydratedAnswers = answers
                .Select(answer =>
                {
                    questionsByAnonymousTemplateQuestionId.TryGetValue(
                        answer.AnonymousTemplateQuestionId,
                        out var question);

                    QuestionOptionForResponseDetailsDto? selectedOption = null;

                    if (answer.SelectedQuestionOptionId.HasValue)
                    {
                        selectedOptionsById.TryGetValue(
                            answer.SelectedQuestionOptionId.Value,
                            out selectedOption);
                    }

                    return answer with
                    {
                        QuestionTextEn = question?.QuestionTextEn ?? string.Empty,
                        QuestionTextAr = question?.QuestionTextAr,
                        QuestionType = question?.QuestionType ?? answer.QuestionType,
                        QuestionTypeName = (question?.QuestionType ?? answer.QuestionType).ToString(),
                        QuestionOrder = question?.QuestionOrder ?? 0,

                        SelectedOptionTextEn = selectedOption?.TextEn,
                        SelectedOptionTextAr = selectedOption?.TextAr,
                        SelectedOptionValue = selectedOption?.Value
                    };
                })
                .OrderBy(x => x.QuestionOrder)
                .ToArray();

            var response = new GetAnonymousTemplateResponseDetailsResponse
            {
                AnonymousSurveyResponseId = responseBasic.AnonymousSurveyResponseId,
                AnonymousTemplateId = responseBasic.AnonymousTemplateId,
                SubmittedOnUtc = responseBasic.SubmittedOnUtc,
                ActualScore = responseBasic.ActualScore,
                MaxScore = responseBasic.MaxScore,
                ScorePercentage = responseBasic.ScorePercentage,
                IsScored = responseBasic.MaxScore > 0,
                AnswersCount = responseBasic.AnswersCount,
                CustomInputValuesCount = responseBasic.CustomInputValuesCount,
                CustomInputValues = customInputValues,
                Answers = hydratedAnswers
            };

            return Result<GetAnonymousTemplateResponseDetailsResponse>.Ok(response);
        }

        private async Task<CurrentBranchActorForGetAnonymousTemplateResponseDetailsDto?> ResolveBranchActorAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var currentBranchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForAnonymousTemplateResponseDetailsSpec(applicationUserId),
                cancellationToken);

            if (currentBranchAdmin is not null)
            {
                return currentBranchAdmin;
            }

            var currentBranchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForAnonymousTemplateResponseDetailsSpec(applicationUserId),
                cancellationToken);

            return currentBranchUser;
        }
    }
}
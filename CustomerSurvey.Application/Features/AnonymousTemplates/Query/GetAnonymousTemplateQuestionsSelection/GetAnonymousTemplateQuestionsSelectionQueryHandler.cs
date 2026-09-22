using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateQuestionsSelection
{
    internal sealed class GetAnonymousTemplateQuestionsSelectionQueryHandler
        : IQueryHandler<GetAnonymousTemplateQuestionsSelectionQuery, GetAnonymousTemplateQuestionsSelectionResponse>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionReadRepository;
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;

        public GetAnonymousTemplateQuestionsSelectionQueryHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteReadRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionReadRepository,
            IWriteReadRepository<Question> questionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser)
        {
            _anonymousTemplateReadRepository = anonymousTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

            _anonymousTemplateQuestionReadRepository = anonymousTemplateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateQuestionReadRepository));

            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _questionOptionReadRepository = questionOptionReadRepository
                ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<GetAnonymousTemplateQuestionsSelectionResponse>> Handle(
            GetAnonymousTemplateQuestionsSelectionQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<GetAnonymousTemplateQuestionsSelectionResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.QuestionsSelection.Unauthenticated",
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
                var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                    cancellationToken);

                if (currentBranchScope.IsFailure)
                {
                    return Result<GetAnonymousTemplateQuestionsSelectionResponse>.Fail(
                        currentBranchScope.Errors);
                }

                currentBranchId = currentBranchScope.Value.BranchId;
            }

            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForQuestionsSelectionSpec(
                    request.AnonymousTemplateId,
                    isSuperAdmin,
                    currentBranchId),
                cancellationToken);

            if (anonymousTemplate is null)
            {
                return Result<GetAnonymousTemplateQuestionsSelectionResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.QuestionsSelection.TemplateNotFound",
                    Message: ErrorMessage.GetAnonymousTemplateQuestionsSelection_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!anonymousTemplate.IsActive)
            {
                return Result<GetAnonymousTemplateQuestionsSelectionResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.QuestionsSelection.TemplateInactive",
                    Message: ErrorMessage.GetAnonymousTemplateQuestionsSelection_Template_Inactive,
                    Type: ErrorType.Validation));
            }

            var selectedQuestions = await _anonymousTemplateQuestionReadRepository.ListAsync(
                new GetSelectedAnonymousTemplateQuestionsForSelectionSpec(
                    anonymousTemplate.AnonymousTemplateId),
                cancellationToken);

            var selectedQuestionsByQuestionId = selectedQuestions
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => x.First());

            var availableQuestions = await _questionReadRepository.ListAsync(
                new GetAvailableQuestionsForAnonymousTemplateSelectionSpec(
                    anonymousTemplate,
                    request.SearchText),
                cancellationToken);

            var singleChoiceQuestionIds = availableQuestions
                .Where(x => x.Type == QuestionType.SingleChoice)
                .Select(x => x.QuestionId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<AnonymousTemplateQuestionSelectionOptionResponse> options;

            if (singleChoiceQuestionIds.Length == 0)
            {
                options = Array.Empty<AnonymousTemplateQuestionSelectionOptionResponse>();
            }
            else
            {
                options = await _questionOptionReadRepository.ListAsync(
                    new GetQuestionOptionsForAnonymousTemplateSelectionSpec(
                        singleChoiceQuestionIds),
                    cancellationToken);
            }

            var optionsByQuestionId = options
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<AnonymousTemplateQuestionSelectionOptionResponse>)x
                        .OrderBy(option => option.Order)
                        .ToArray());

            var selectionItems = availableQuestions
                .Select(question =>
                {
                    selectedQuestionsByQuestionId.TryGetValue(
                        question.QuestionId,
                        out var selectedQuestion);

                    optionsByQuestionId.TryGetValue(
                        question.QuestionId,
                        out var questionOptions);

                    return question with
                    {
                        AnonymousTemplateQuestionId = selectedQuestion?.AnonymousTemplateQuestionId,
                        IsSelected = selectedQuestion is not null,
                        SelectedOrder = selectedQuestion?.Order,
                        Options = question.Type == QuestionType.SingleChoice
                            ? questionOptions ?? Array.Empty<AnonymousTemplateQuestionSelectionOptionResponse>()
                            : Array.Empty<AnonymousTemplateQuestionSelectionOptionResponse>()
                    };
                })
                .OrderByDescending(x => x.IsSelected)
                .ThenBy(x => x.SelectedOrder ?? int.MaxValue)
                .ThenBy(x => x.GroupNameEn)
                .ThenBy(x => x.TextEn)
                .ToArray();

            var response = new GetAnonymousTemplateQuestionsSelectionResponse
            {
                AnonymousTemplateId = anonymousTemplate.AnonymousTemplateId,
                BranchId = anonymousTemplate.BranchId,
                Scope = anonymousTemplate.Scope,
                ScopeName = anonymousTemplate.Scope.ToString(),
                IsGlobal = anonymousTemplate.Scope == AnonymousTemplateScope.Global,
                NameEn = anonymousTemplate.NameEn,
                NameAr = anonymousTemplate.NameAr,
                LogoPath = anonymousTemplate.LogoPath,
                BranchNameEn = anonymousTemplate.BranchNameEn,
                BranchNameAr = anonymousTemplate.BranchNameAr,
                SelectedQuestionsCount = selectedQuestions.Count,
                Questions = selectionItems
            };

            return Result<GetAnonymousTemplateQuestionsSelectionResponse>.Ok(response);
        }

    }
}

using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignQuestionsToAnonymousTemplate
{
    internal sealed class AssignQuestionsToAnonymousTemplateCommandHandler
        : ICommandHandler<AssignQuestionsToAnonymousTemplateCommand, AssignQuestionsToAnonymousTemplateResponse>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionReadRepository;
        private readonly IWriteRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionWriteRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestionCondition> _conditionReadRepository;
        private readonly IWriteRepository<AnonymousTemplateQuestionCondition> _conditionWriteRepository;
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public AssignQuestionsToAnonymousTemplateCommandHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteReadRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionReadRepository,
            IWriteRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionWriteRepository,
            IWriteReadRepository<AnonymousTemplateQuestionCondition> conditionReadRepository,
            IWriteRepository<AnonymousTemplateQuestionCondition> conditionWriteRepository,
            IWriteReadRepository<Question> questionReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _anonymousTemplateReadRepository = anonymousTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

            _anonymousTemplateQuestionReadRepository = anonymousTemplateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateQuestionReadRepository));

            _anonymousTemplateQuestionWriteRepository = anonymousTemplateQuestionWriteRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateQuestionWriteRepository));

            _conditionReadRepository = conditionReadRepository
                ?? throw new ArgumentNullException(nameof(conditionReadRepository));

            _conditionWriteRepository = conditionWriteRepository
                ?? throw new ArgumentNullException(nameof(conditionWriteRepository));

            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<AssignQuestionsToAnonymousTemplateResponse>> Handle(
            AssignQuestionsToAnonymousTemplateCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<AssignQuestionsToAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.AssignQuestions.Unauthenticated",
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
                    return Result<AssignQuestionsToAnonymousTemplateResponse>.Fail(currentBranchScope.Errors);
                }

                currentBranchId = currentBranchScope.Value.BranchId;
            }

            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForAssignQuestionsSpec(
                    request.AnonymousTemplateId,
                    isSuperAdmin,
                    currentBranchId),
                cancellationToken);

            if (anonymousTemplate is null)
            {
                return Result<AssignQuestionsToAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.AssignQuestions.TemplateNotFound",
                    Message: ErrorMessage.AssignQuestionsToAnonymousTemplate_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (anonymousTemplate.IsArchived)
            {
                return Result<AssignQuestionsToAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.AssignQuestions.TemplateArchived",
                    Message: "The global anonymous template must be restored before its structure can be changed.",
                    Type: ErrorType.Validation));
            }

            if (anonymousTemplate.SourceGlobalAnonymousTemplateId.HasValue)
            {
                return Result<AssignQuestionsToAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.AssignQuestions.ManagedCopyReadOnly",
                    Message: "A managed global branch copy has read-only survey structure.",
                    Type: ErrorType.Validation));
            }

            if (anonymousTemplate.IsBranchScoped && !anonymousTemplate.IsActive)
            {
                return Result<AssignQuestionsToAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.AssignQuestions.TemplateInactive",
                    Message: ErrorMessage.AssignQuestionsToAnonymousTemplate_Template_Inactive,
                    Type: ErrorType.Validation));
            }

            var existingAssignments = await _anonymousTemplateQuestionReadRepository.ListAsync(
                new GetExistingAnonymousTemplateQuestionsForAssignSpec(anonymousTemplate.Id),
                cancellationToken);

            var requestedQuestions = request.Questions
                .OrderBy(x => x.Order)
                .ToArray();

            var requestedQuestionIds = requestedQuestions
                .Select(x => x.QuestionId)
                .Distinct()
                .ToArray();

            var questionValidationResult = await ValidateRequestedQuestionsAsync(
                anonymousTemplate,
                requestedQuestionIds,
                cancellationToken);

            if (questionValidationResult.Error is not null)
            {
                return Result<AssignQuestionsToAnonymousTemplateResponse>.Fail(
                    questionValidationResult.Error);
            }

            var questionsById = questionValidationResult.Questions
                .ToDictionary(x => x.QuestionId);

            var existingByQuestionId = existingAssignments
                .ToDictionary(x => x.QuestionId);

            var requestedQuestionIdSet = requestedQuestionIds.ToHashSet();

            var assignmentsToRemove = existingAssignments
                .Where(x => !requestedQuestionIdSet.Contains(x.QuestionId))
                .ToArray();

            if (assignmentsToRemove.Length > 0)
            {
                var removedAnonymousTemplateQuestionIds = assignmentsToRemove
                    .Select(x => x.Id)
                    .ToArray();

                var relatedConditions = await _conditionReadRepository.ListAsync(
                    new GetAnonymousTemplateConditionsRelatedToRemovedQuestionsSpec(
                        anonymousTemplate.Id,
                        removedAnonymousTemplateQuestionIds),
                    cancellationToken);

                if (relatedConditions.Count > 0)
                {
                    _conditionWriteRepository.DeleteRange(relatedConditions);
                }

                _anonymousTemplateQuestionWriteRepository.DeleteRange(assignmentsToRemove);
            }

            var assignmentsToUpdate = new List<AnonymousTemplateQuestion>();
            var assignmentsToAdd = new List<AnonymousTemplateQuestion>();

            foreach (var requestedQuestion in requestedQuestions)
            {
                if (existingByQuestionId.TryGetValue(
                        requestedQuestion.QuestionId,
                        out var existingAssignment))
                {
                    existingAssignment.ChangeOrder(requestedQuestion.Order);
                    assignmentsToUpdate.Add(existingAssignment);
                    continue;
                }

                var newAssignment = AnonymousTemplateQuestion.Create(
                    anonymousTemplateId: anonymousTemplate.Id,
                    questionId: requestedQuestion.QuestionId,
                    order: requestedQuestion.Order,
                    createdByApplicationUserId: currentApplicationUserId);

                assignmentsToAdd.Add(newAssignment);
            }

            if (assignmentsToUpdate.Count > 0)
            {
                _anonymousTemplateQuestionWriteRepository.UpdateRange(assignmentsToUpdate);
            }

            if (assignmentsToAdd.Count > 0)
            {
                await _anonymousTemplateQuestionWriteRepository.AddRangeAsync(
                    assignmentsToAdd,
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var finalAssignments = existingAssignments
                .Except(assignmentsToRemove)
                .Concat(assignmentsToAdd)
                .OrderBy(x => x.Order)
                .ToArray();

            var responseQuestions = finalAssignments
                .Select(x =>
                {
                    var question = questionsById[x.QuestionId];

                    return new AssignedAnonymousTemplateQuestionResponse
                    {
                        AnonymousTemplateQuestionId = x.Id,
                        QuestionId = x.QuestionId,
                        BranchId = question.BranchId,
                        GroupId = question.GroupId,
                        GroupNameEn = question.GroupNameEn,
                        GroupNameAr = question.GroupNameAr,
                        Scope = question.Scope,
                        ScopeName = question.Scope.ToString(),
                        IsGlobal = question.Scope == QuestionScope.Global,
                        TextEn = question.TextEn,
                        TextAr = question.TextAr,
                        Type = question.Type,
                        TypeName = question.Type.ToString(),
                        Order = x.Order
                    };
                })
                .ToArray();

            return Result<AssignQuestionsToAnonymousTemplateResponse>.Ok(
                new AssignQuestionsToAnonymousTemplateResponse
                {
                    AnonymousTemplateId = anonymousTemplate.Id,
                    BranchId = anonymousTemplate.BranchId,
                    Scope = anonymousTemplate.Scope,
                    ScopeName = anonymousTemplate.Scope.ToString(),
                    IsGlobal = anonymousTemplate.Scope == AnonymousTemplateScope.Global,
                    AssignedQuestionsCount = responseQuestions.Length,
                    Questions = responseQuestions
                });
        }

        private async Task<QuestionValidationResult> ValidateRequestedQuestionsAsync(
            AnonymousTemplate anonymousTemplate,
            IReadOnlyCollection<Guid> requestedQuestionIds,
            CancellationToken cancellationToken)
        {
            if (requestedQuestionIds.Count == 0)
            {
                return QuestionValidationResult.Ok(Array.Empty<QuestionForAssignToAnonymousTemplateDto>());
            }

            var questions = await _questionReadRepository.ListAsync(
                new GetQuestionsForAssignAnonymousTemplateSpec(requestedQuestionIds),
                cancellationToken);

            if (questions.Count != requestedQuestionIds.Count)
            {
                return QuestionValidationResult.Fail(new Error(
                    Code: "AnonymousTemplates.AssignQuestions.QuestionNotFound",
                    Message: ErrorMessage.AssignQuestionsToAnonymousTemplate_Question_NotFound,
                    Type: ErrorType.NotFound));
            }

            foreach (var question in questions)
            {
                if (!question.IsActive || !question.GroupIsActive)
                {
                    return QuestionValidationResult.Fail(new Error(
                        Code: "AnonymousTemplates.AssignQuestions.QuestionInactive",
                        Message: ErrorMessage.AssignQuestionsToAnonymousTemplate_Question_Inactive,
                        Type: ErrorType.Validation));
                }

                var isAllowed = IsQuestionAllowedForTemplate(
                    anonymousTemplate,
                    question);

                if (!isAllowed)
                {
                    return QuestionValidationResult.Fail(new Error(
                        Code: "AnonymousTemplates.AssignQuestions.QuestionNotAllowed",
                        Message: anonymousTemplate.Scope == AnonymousTemplateScope.Global
                            ? ErrorMessage.AssignQuestionsToAnonymousTemplate_GlobalTemplate_GlobalQuestionsOnly
                            : ErrorMessage.AssignQuestionsToAnonymousTemplate_Question_NotAllowed,
                        Type: ErrorType.Validation));
                }
            }

            return QuestionValidationResult.Ok(questions);
        }

        private static bool IsQuestionAllowedForTemplate(
            AnonymousTemplate anonymousTemplate,
            QuestionForAssignToAnonymousTemplateDto question)
        {
            if (anonymousTemplate.Scope == AnonymousTemplateScope.Global)
            {
                return question.Scope == QuestionScope.Global;
            }

            return question.Scope == QuestionScope.Global ||
                   (
                       question.Scope == QuestionScope.Branch &&
                       question.BranchId == anonymousTemplate.BranchId
                   );
        }

        private sealed record QuestionValidationResult(
            Error? Error,
            IReadOnlyCollection<QuestionForAssignToAnonymousTemplateDto> Questions)
        {
            public static QuestionValidationResult Ok(
                IReadOnlyCollection<QuestionForAssignToAnonymousTemplateDto> questions)
            {
                return new QuestionValidationResult(
                    Error: null,
                    Questions: questions);
            }

            public static QuestionValidationResult Fail(Error error)
            {
                return new QuestionValidationResult(
                    Error: error,
                    Questions: Array.Empty<QuestionForAssignToAnonymousTemplateDto>());
            }
        }
    }
}

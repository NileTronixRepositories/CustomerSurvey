using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate
{
    internal sealed class AssignQuestionsToTemplateCommandHandler
        : ICommandHandler<AssignQuestionsToTemplateCommand, AssignQuestionsToTemplateResponse>
    {
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionReadRepository;
        private readonly IWriteRepository<TemplateQuestion> _templateQuestionWriteRepository;
        private readonly IWriteReadRepository<TemplateQuestionCondition> _conditionReadRepository;
        private readonly IWriteRepository<TemplateQuestionCondition> _conditionWriteRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public AssignQuestionsToTemplateCommandHandler(
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<Template> templateReadRepository,
            IWriteReadRepository<Question> questionReadRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
            IWriteRepository<TemplateQuestion> templateQuestionWriteRepository,
            IWriteReadRepository<TemplateQuestionCondition> conditionReadRepository,
            IWriteRepository<TemplateQuestionCondition> conditionWriteRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _templateQuestionReadRepository = templateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(templateQuestionReadRepository));

            _templateQuestionWriteRepository = templateQuestionWriteRepository
                ?? throw new ArgumentNullException(nameof(templateQuestionWriteRepository));

            _conditionReadRepository = conditionReadRepository
                ?? throw new ArgumentNullException(nameof(conditionReadRepository));

            _conditionWriteRepository = conditionWriteRepository
                ?? throw new ArgumentNullException(nameof(conditionWriteRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<AssignQuestionsToTemplateResponse>> Handle(
            AssignQuestionsToTemplateCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<AssignQuestionsToTemplateResponse>.Fail(new Error(
                    Code: "Templates.AssignQuestions.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentActor = await ResolveCurrentBranchActorAsync(
                currentApplicationUserId,
                cancellationToken);

            if (currentActor is null)
            {
                return Result<AssignQuestionsToTemplateResponse>.Fail(new Error(
                    Code: "Templates.AssignQuestions.CurrentBranchActorNotFound",
                    Message: ErrorMessage.AssignQuestionsToTemplate_CurrentBranchActor_NotFound,
                    Type: ErrorType.NotFound));
            }

            var branchId = currentActor.BranchId;

            var template = await _templateReadRepository.FirstOrDefaultAsync(
                new GetTemplateForAssignQuestionsToTemplateSpec(
                    templateId: request.TemplateId,
                    branchId: branchId),
                cancellationToken);

            if (template is null)
            {
                return Result<AssignQuestionsToTemplateResponse>.Fail(new Error(
                    Code: "Templates.AssignQuestions.TemplateNotFound",
                    Message: ErrorMessage.AssignQuestionsToTemplate_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!template.IsActive)
            {
                return Result<AssignQuestionsToTemplateResponse>.Fail(new Error(
                    Code: "Templates.AssignQuestions.TemplateInactive",
                    Message: ErrorMessage.AssignQuestionsToTemplate_Template_Inactive,
                    Type: ErrorType.Validation));
            }

            var requestedQuestionIds = request.QuestionIds
                .Distinct()
                .ToArray();

            var requestedOrderByQuestionId = request.QuestionIds
                .Select((questionId, index) => new
                {
                    QuestionId = questionId,
                    Order = index + 1
                })
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => x.First().Order);

            var questions = await _questionReadRepository.ListAsync(
                new GetQuestionsForAssignQuestionsToTemplateSpec(
                    questionIds: requestedQuestionIds,
                    branchId: branchId),
                cancellationToken);

            if (questions.Count != requestedQuestionIds.Length)
            {
                return Result<AssignQuestionsToTemplateResponse>.Fail(new Error(
                    Code: "Templates.AssignQuestions.QuestionNotFound",
                    Message: ErrorMessage.AssignQuestionsToTemplate_Question_NotFound,
                    Type: ErrorType.NotFound));
            }

            var questionsById = questions.ToDictionary(x => x.QuestionId);

            var existingTemplateQuestions = await _templateQuestionReadRepository.ListAsync(
                new GetTemplateQuestionsForAssignQuestionsToTemplateSpec(template.TemplateId),
                cancellationToken);

            var existingByQuestionId = existingTemplateQuestions
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => x.First());

            var removedTemplateQuestions = existingTemplateQuestions
                .Where(x => !requestedQuestionIds.Contains(x.QuestionId))
                .ToArray();

            await DeleteConditionsForRemovedTemplateQuestionsAsync(
                removedTemplateQuestions,
                cancellationToken);

            foreach (var removedTemplateQuestion in removedTemplateQuestions)
            {
                _templateQuestionWriteRepository.Delete(removedTemplateQuestion);
            }

            var assignedTemplateQuestionsByQuestionId = existingTemplateQuestions
                .Where(x => requestedQuestionIds.Contains(x.QuestionId))
                .ToDictionary(
                    x => x.QuestionId,
                    x => x);

            foreach (var item in requestedOrderByQuestionId)
            {
                var questionId = item.Key;
                var order = item.Value;

                if (existingByQuestionId.TryGetValue(questionId, out var existingTemplateQuestion))
                {
                    existingTemplateQuestion.ChangeOrder(order);

                    _templateQuestionWriteRepository.Update(existingTemplateQuestion);

                    assignedTemplateQuestionsByQuestionId[questionId] = existingTemplateQuestion;

                    continue;
                }

                var templateQuestion = TemplateQuestion.Create(
                    templateId: template.TemplateId,
                    questionId: questionId,
                    order: order,
                    createdByApplicationUserId: currentApplicationUserId);

                await _templateQuestionWriteRepository.AddAsync(
                    templateQuestion,
                    cancellationToken);

                assignedTemplateQuestionsByQuestionId[questionId] = templateQuestion;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new AssignQuestionsToTemplateResponse
            {
                TemplateId = template.TemplateId,
                BranchId = template.BranchId,
                QuestionsCount = assignedTemplateQuestionsByQuestionId.Count,
                Questions = assignedTemplateQuestionsByQuestionId
                    .Values
                    .OrderBy(templateQuestion => templateQuestion.Order)
                    .Select(templateQuestion =>
                    {
                        var question = questionsById[templateQuestion.QuestionId];

                        return new AssignedTemplateQuestionResponse
                        {
                            TemplateQuestionId = templateQuestion.Id,
                            QuestionId = templateQuestion.QuestionId,
                            QuestionBranchId = question.BranchId,
                            GroupId = question.GroupId,
                            Scope = question.Scope,
                            ScopeName = question.Scope.ToString(),
                            IsGlobal = question.Scope == QuestionScope.Global,
                            Order = templateQuestion.Order
                        };
                    })
                    .ToArray()
            };

            return Result<AssignQuestionsToTemplateResponse>.Ok(response);
        }

        private async Task DeleteConditionsForRemovedTemplateQuestionsAsync(
            IReadOnlyCollection<TemplateQuestion> removedTemplateQuestions,
            CancellationToken cancellationToken)
        {
            if (removedTemplateQuestions.Count == 0)
            {
                return;
            }

            var removedTemplateQuestionIds = removedTemplateQuestions
                .Select(x => x.Id)
                .Distinct()
                .ToArray();

            var relatedConditions = await _conditionReadRepository.ListAsync(
                new GetTemplateQuestionConditionsByTemplateQuestionIdsSpec(
                    removedTemplateQuestionIds),
                cancellationToken);

            foreach (var condition in relatedConditions)
            {
                _conditionWriteRepository.Delete(condition);
            }
        }

        private async Task<CurrentBranchActorForAssignQuestionsToTemplateDto?> ResolveCurrentBranchActorAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForAssignQuestionsToTemplateSpec(applicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return branchAdmin;
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForAssignQuestionsToTemplateSpec(applicationUserId),
                cancellationToken);

            return branchUser;
        }
    }
}
using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public AssignQuestionsToTemplateCommandHandler(
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<Template> templateReadRepository,
            IWriteReadRepository<Question> questionReadRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
            IWriteRepository<TemplateQuestion> templateQuestionWriteRepository,
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
                    Type: ErrorType.Security));
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

            if (!template.IsActive || template.Status == TemplateStatus.Inactive)
            {
                return Result<AssignQuestionsToTemplateResponse>.Fail(new Error(
                    Code: "Templates.AssignQuestions.TemplateInactive",
                    Message: ErrorMessage.AssignQuestionsToTemplate_Template_Inactive,
                    Type: ErrorType.Validation));
            }

            var requestedQuestionIds = request.QuestionIds
                .Distinct()
                .ToArray();

            var questions = await _questionReadRepository.ListAsync(
                new GetQuestionsForAssignQuestionsToTemplateSpec(
                    questionIds: requestedQuestionIds,
                    branchId: branchId),
                cancellationToken);

            if (questions.Count != requestedQuestionIds.Length)
            {
                return Result<AssignQuestionsToTemplateResponse>.Fail(new Error(
                    Code: "Templates.AssignQuestions.SomeQuestionsNotFoundOrInactive",
                    Message: ErrorMessage.AssignQuestionsToTemplate_Questions_NotFoundOrInactive,
                    Type: ErrorType.Validation));
            }

            var existingTemplateQuestions = await _templateQuestionReadRepository.ListAsync(
                new GetTemplateQuestionsForAssignQuestionsToTemplateSpec(template.TemplateId),
                cancellationToken);

            var requestedOrderByQuestionId = requestedQuestionIds
                .Select((questionId, index) => new
                {
                    QuestionId = questionId,
                    Order = index + 1
                })
                .ToDictionary(x => x.QuestionId, x => x.Order);

            var existingByQuestionId = existingTemplateQuestions
                .GroupBy(x => x.QuestionId)
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var existingTemplateQuestion in existingTemplateQuestions)
            {
                if (!requestedOrderByQuestionId.ContainsKey(existingTemplateQuestion.QuestionId))
                {
                    _templateQuestionWriteRepository.Delete(existingTemplateQuestion);
                }
            }

            foreach (var item in requestedOrderByQuestionId)
            {
                if (existingByQuestionId.TryGetValue(item.Key, out var existingTemplateQuestion))
                {
                    existingTemplateQuestion.ChangeOrder(item.Value);
                    _templateQuestionWriteRepository.Update(existingTemplateQuestion);
                    continue;
                }

                var templateQuestion = TemplateQuestion.Create(
      templateId: template.TemplateId,
      questionId: item.Key,
      order: item.Value,
      createdByApplicationUserId: currentApplicationUserId);

                await _templateQuestionWriteRepository.AddAsync(
                    templateQuestion,
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new AssignQuestionsToTemplateResponse
            {
                TemplateId = template.TemplateId,
                BranchId = template.BranchId,
                QuestionsCount = requestedQuestionIds.Length,
                Questions = requestedOrderByQuestionId
                    .OrderBy(x => x.Value)
                    .Select(x => new AssignedTemplateQuestionResponse
                    {
                        QuestionId = x.Key,
                        Order = x.Value
                    })
                    .ToArray()
            };

            return Result<AssignQuestionsToTemplateResponse>.Ok(response);
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
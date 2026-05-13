using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Application.Features.Questions.Shared.Specs;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion
{
    internal sealed class UpdateQuestionCommandHandler
        : ICommandHandler<UpdateQuestionCommand, UpdateQuestionResponse>
    {
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteRepository<Question> _questionWriteRepository;
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteRepository<QuestionOption> _questionOptionWriteRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateQuestionCommandHandler(
            IWriteReadRepository<Question> questionReadRepository,
            IWriteRepository<Question> questionWriteRepository,
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteRepository<QuestionOption> questionOptionWriteRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _questionWriteRepository = questionWriteRepository
                ?? throw new ArgumentNullException(nameof(questionWriteRepository));

            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _questionOptionReadRepository = questionOptionReadRepository
                ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));

            _questionOptionWriteRepository = questionOptionWriteRepository
                ?? throw new ArgumentNullException(nameof(questionOptionWriteRepository));

            _templateQuestionReadRepository = templateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(templateQuestionReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<UpdateQuestionResponse>> Handle(
            UpdateQuestionCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<UpdateQuestionResponse>.Fail(new Error(
                    Code: "Questions.Update.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorBranchId = await ResolveActorBranchIdAsync(
                currentApplicationUserId,
                cancellationToken);

            if (!actorBranchId.HasValue)
            {
                return Result<UpdateQuestionResponse>.Fail(new Error(
                    Code: "Questions.Update.CurrentBranchActorNotFound",
                    Message: ErrorMessage.UpdateQuestion_CurrentBranchActor_NotFound,
                    Type: ErrorType.Security));
            }

            var branchId = actorBranchId.Value;

            var question = await _questionReadRepository.FirstOrDefaultAsync(
                new GetQuestionForUpdateSpec(
                    questionId: request.QuestionId,
                    branchId: branchId),
                cancellationToken);

            if (question is null)
            {
                return Result<UpdateQuestionResponse>.Fail(new Error(
                    Code: "Questions.Update.QuestionNotFound",
                    Message: ErrorMessage.UpdateQuestion_Question_NotFound,
                    Type: ErrorType.NotFound));
            }

            var group = await _questionGroupReadRepository.FirstOrDefaultAsync(
                new GetQuestionGroupForUpdateQuestionSpec(
                    groupId: request.GroupId,
                    branchId: branchId),
                cancellationToken);

            if (group is null)
            {
                return Result<UpdateQuestionResponse>.Fail(new Error(
                    Code: "Questions.Update.QuestionGroupNotFound",
                    Message: ErrorMessage.UpdateQuestion_QuestionGroup_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!group.IsActive)
            {
                return Result<UpdateQuestionResponse>.Fail(new Error(
                    Code: "Questions.Update.QuestionGroupInactive",
                    Message: ErrorMessage.UpdateQuestion_QuestionGroup_Inactive,
                    Type: ErrorType.Validation));
            }

            var isAssignedToTemplate = await _templateQuestionReadRepository.AnyAsync(
                x => x.QuestionId == question.Id,
                cancellationToken);

            if (isAssignedToTemplate && question.Type != request.Type)
            {
                return Result<UpdateQuestionResponse>.Fail(new Error(
                    Code: "Questions.Update.TypeCannotChangeAfterTemplateAssignment",
                    Message: ErrorMessage.UpdateQuestion_Type_CannotChangeAfterTemplateAssignment,
                    Type: ErrorType.Validation));
            }

            question.Update(
                groupId: group.Id,
                textEn: request.TextEn,
                textAr: request.TextAr,
                type: request.Type);

            _questionWriteRepository.Update(question);

            var currentOptions = await _questionOptionReadRepository.ListAsync(
                new GetQuestionOptionsForUpdateQuestionSpec(question.Id),
                cancellationToken);

            foreach (var option in currentOptions)
            {
                _questionOptionWriteRepository.Delete(option);
            }

            var optionResponses = Array.Empty<QuestionOptionResponse>();

            if (request.Type == QuestionType.SingleChoice)
            {
                var newOptions = request.Options
                    .OrderBy(x => x.Order)
                    .Select(option => QuestionOption.Create(
                        questionId: question.Id,
                        textEn: option.TextEn,
                        textAr: option.TextAr,
                        order: option.Order,
                        createdByApplicationUserId: currentApplicationUserId))
                    .ToArray();

                foreach (var option in newOptions)
                {
                    await _questionOptionWriteRepository.AddAsync(option, cancellationToken);
                }

                optionResponses = newOptions
                    .Select(option => new QuestionOptionResponse
                    {
                        OptionId = option.Id,
                        QuestionId = option.QuestionId,
                        TextEn = option.TextEn,
                        TextAr = option.TextAr,
                        Order = option.Order,
                        IsActive = option.IsActive
                    })
                    .ToArray();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new UpdateQuestionResponse
            {
                QuestionId = question.Id,
                BranchId = question.BranchId,
                GroupId = question.GroupId,
                TextEn = question.TextEn,
                TextAr = question.TextAr,
                Type = question.Type,
                TypeName = question.Type.ToString(),
                IsActive = question.IsActive,
                Options = optionResponses
            };

            return Result<UpdateQuestionResponse>.Ok(response);
        }

        private async Task<Guid?> ResolveActorBranchIdAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForUpdateQuestionSpec(applicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return branchAdmin.BranchId;
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForUpdateQuestionSpec(applicationUserId),
                cancellationToken);

            return branchUser?.BranchId;
        }
    }
}
using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.QuestionGroups.Shared.Specs;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Questions.Command.CreateQuestion
{
    internal sealed class CreateQuestionCommandHandler
        : ICommandHandler<CreateQuestionCommand, CreateQuestionResponse>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteRepository<Question> _questionWriteRepository;
        private readonly IWriteRepository<QuestionOption> _questionOptionWriteRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateQuestionCommandHandler(
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteRepository<Question> questionWriteRepository,
            IWriteRepository<QuestionOption> questionOptionWriteRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _questionWriteRepository = questionWriteRepository
                ?? throw new ArgumentNullException(nameof(questionWriteRepository));

            _questionOptionWriteRepository = questionOptionWriteRepository
                ?? throw new ArgumentNullException(nameof(questionOptionWriteRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<CreateQuestionResponse>> Handle(
            CreateQuestionCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateQuestionResponse>.Fail(new Error(
                    Code: "Questions.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorBranchId = await ResolveActorBranchIdAsync(
                currentApplicationUserId,
                cancellationToken);

            if (!actorBranchId.HasValue)
            {
                return Result<CreateQuestionResponse>.Fail(new Error(
                    Code: "Questions.Create.CurrentBranchActorNotFound",
                    Message: ErrorMessage.CreateQuestion_CurrentBranchActor_NotFound,
                    Type: ErrorType.Security));
            }

            var branchId = actorBranchId.Value;

            var group = await _questionGroupReadRepository.FirstOrDefaultAsync(
                new GetQuestionGroupForCreateQuestionSpec(
                    groupId: request.GroupId,
                    branchId: branchId),
                cancellationToken);

            if (group is null)
            {
                return Result<CreateQuestionResponse>.Fail(new Error(
                    Code: "Questions.Create.QuestionGroupNotFound",
                    Message: ErrorMessage.CreateQuestion_Group_NotFound,
                    Type: ErrorType.NotFound));
            }

            var question = Question.Create(
                branchId: branchId,
                groupId: group.Id,
                textEn: request.TextEn,
                textAr: request.TextAr,
                type: request.Type,
                createdByApplicationUserId: currentApplicationUserId);

            await _questionWriteRepository.AddAsync(question, cancellationToken);

            IReadOnlyCollection<QuestionOptionResponse> optionResponses =
                Array.Empty<QuestionOptionResponse>();

            if (request.Type == QuestionType.SingleChoice)
            {
                var options = request.Options
                    .Select(option => QuestionOption.Create(
                        questionId: question.Id,
                        textEn: option.TextEn,
                        textAr: option.TextAr,
                        order: option.Order,
                        value: option.Value,
                        createdByApplicationUserId: currentApplicationUserId))
                    .ToArray();

                foreach (var option in options)
                {
                    await _questionOptionWriteRepository.AddAsync(option, cancellationToken);
                }

                optionResponses = options
                    .Select(option => new QuestionOptionResponse
                    {
                        OptionId = option.Id,
                        QuestionId = option.QuestionId,
                        TextEn = option.TextEn,
                        TextAr = option.TextAr,
                        Order = option.Order,
                        Value = option.Value,
                        IsActive = option.IsActive
                    })
                    .ToArray();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateQuestionResponse
            {
                QuestionId = question.Id,
                BranchId = question.BranchId,
                GroupId = question.GroupId,
                GroupBranchId = group.BranchId,
                Scope = question.Scope,
                ScopeName = question.Scope.ToString(),
                IsGlobal = question.Scope == QuestionScope.Global,

                // This endpoint creates branch questions only.
                IsEditable = question.Scope == QuestionScope.Branch,

                TextEn = question.TextEn,
                TextAr = question.TextAr,
                Type = question.Type,
                TypeName = question.Type.ToString(),
                IsActive = question.IsActive,
                Options = optionResponses
            };

            return Result<CreateQuestionResponse>.Ok(response);
        }

        private async Task<Guid?> ResolveActorBranchIdAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForCreateQuestionSpec(applicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return branchAdmin.BranchId;
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForCreateQuestionSpec(applicationUserId),
                cancellationToken);

            return branchUser?.BranchId;
        }
    }
}
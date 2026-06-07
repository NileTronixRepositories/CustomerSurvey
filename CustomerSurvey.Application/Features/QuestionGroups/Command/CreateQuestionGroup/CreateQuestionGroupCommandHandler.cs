using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.QuestionGroups.Shared.Specs;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.CreateQuestionGroup
{
    internal sealed class CreateQuestionGroupCommandHandler
        : ICommandHandler<CreateQuestionGroupCommand, CreateQuestionGroupResponse>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteRepository<QuestionGroup> _questionGroupWriteRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateQuestionGroupCommandHandler(
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteRepository<QuestionGroup> questionGroupWriteRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _questionGroupWriteRepository = questionGroupWriteRepository
                ?? throw new ArgumentNullException(nameof(questionGroupWriteRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<CreateQuestionGroupResponse>> Handle(
            CreateQuestionGroupCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateQuestionGroupResponse>.Fail(new Error(
                    Code: "QuestionGroups.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                cancellationToken);

            if (currentBranchScope.IsFailure)
            {
                return Result<CreateQuestionGroupResponse>.Fail(currentBranchScope.Errors);
            }

            var branchId = currentBranchScope.Value.BranchId;
            var normalizedNameEn = request.NameEn.Trim();

            var nameEnExists = await _questionGroupReadRepository.AnyAsync(
                x =>
                    x.Scope == QuestionScope.Branch &&
                    x.BranchId == branchId &&
                    x.NameEn == normalizedNameEn,
                cancellationToken);

            if (nameEnExists)
            {
                return Result<CreateQuestionGroupResponse>.Fail(new Error(
                    Code: "QuestionGroups.Create.NameEnAlreadyExists",
                    Message: ErrorMessage.CreateQuestionGroup_NameEn_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var group = QuestionGroup.Create(
                branchId: branchId,
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                createdByApplicationUserId: currentApplicationUserId);

            await _questionGroupWriteRepository.AddAsync(group, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateQuestionGroupResponse
            {
                GroupId = group.Id,
                BranchId = group.BranchId,
                Scope = group.Scope,
                ScopeName = group.Scope.ToString(),
                IsGlobal = group.Scope == QuestionScope.Global,
                IsEditable = group.Scope == QuestionScope.Branch,
                NameEn = group.NameEn,
                NameAr = group.NameAr,
                IsActive = group.IsActive
            };

            return Result<CreateQuestionGroupResponse>.Ok(response);
        }

    }
}

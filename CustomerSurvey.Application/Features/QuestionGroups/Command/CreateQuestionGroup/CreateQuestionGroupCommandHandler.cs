using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.QuestionGroups.Shared.Specs;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.CreateQuestionGroup
{
    internal sealed class CreateQuestionGroupCommandHandler
     : ICommandHandler<CreateQuestionGroupCommand, CreateQuestionGroupResponse>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteRepository<QuestionGroup> _questionGroupWriteRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateQuestionGroupCommandHandler(
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteRepository<QuestionGroup> questionGroupWriteRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _questionGroupWriteRepository = questionGroupWriteRepository
                ?? throw new ArgumentNullException(nameof(questionGroupWriteRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

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

            var actorBranchId = await ResolveActorBranchIdAsync(
                currentApplicationUserId,
                cancellationToken);

            if (!actorBranchId.HasValue)
            {
                return Result<CreateQuestionGroupResponse>.Fail(new Error(
                    Code: "QuestionGroups.Create.CurrentBranchActorNotFound",
                    Message: ErrorMessage.CreateQuestionGroup_CurrentBranchActor_NotFound,
                    Type: ErrorType.Security));
            }

            var branchId = actorBranchId.Value;
            var normalizedNameEn = request.NameEn.Trim();

            var nameEnExists = await _questionGroupReadRepository.AnyAsync(
                x => x.BranchId == branchId && x.NameEn == normalizedNameEn,
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
                NameEn = group.NameEn,
                NameAr = group.NameAr,
                IsActive = group.IsActive
            };

            return Result<CreateQuestionGroupResponse>.Ok(response);
        }

        private async Task<Guid?> ResolveActorBranchIdAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForQuestionGroupSpec(applicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return branchAdmin.BranchId;
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForQuestionGroupSpec(applicationUserId),
                cancellationToken);

            return branchUser?.BranchId;
        }
    }
}
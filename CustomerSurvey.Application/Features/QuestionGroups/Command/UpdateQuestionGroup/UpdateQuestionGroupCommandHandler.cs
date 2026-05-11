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

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.UpdateQuestionGroup
{
    internal sealed class UpdateQuestionGroupCommandHandler
        : ICommandHandler<UpdateQuestionGroupCommand, UpdateQuestionGroupResponse>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteRepository<QuestionGroup> _questionGroupWriteRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateQuestionGroupCommandHandler(
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

        public async Task<Result<UpdateQuestionGroupResponse>> Handle(
            UpdateQuestionGroupCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<UpdateQuestionGroupResponse>.Fail(new Error(
                    Code: "QuestionGroups.Update.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorBranchId = await ResolveActorBranchIdAsync(
                currentApplicationUserId,
                cancellationToken);

            if (!actorBranchId.HasValue)
            {
                return Result<UpdateQuestionGroupResponse>.Fail(new Error(
                    Code: "QuestionGroups.Update.CurrentBranchActorNotFound",
                    Message: ErrorMessage.UpdateQuestionGroup_CurrentBranchActor_NotFound,
                    Type: ErrorType.Security));
            }

            var branchId = actorBranchId.Value;

            var group = await _questionGroupReadRepository.FirstOrDefaultAsync(
                new GetQuestionGroupForUpdateSpec(
                    groupId: request.GroupId,
                    branchId: branchId),
                cancellationToken);

            if (group is null)
            {
                return Result<UpdateQuestionGroupResponse>.Fail(new Error(
                    Code: "QuestionGroups.Update.QuestionGroupNotFound",
                    Message: ErrorMessage.UpdateQuestionGroup_QuestionGroup_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedNameEn = request.NameEn.Trim();

            var nameEnExists = await _questionGroupReadRepository.AnyAsync(
                x =>
                    x.BranchId == branchId &&
                    x.Id != request.GroupId &&
                    x.NameEn == normalizedNameEn,
                cancellationToken);

            if (nameEnExists)
            {
                return Result<UpdateQuestionGroupResponse>.Fail(new Error(
                    Code: "QuestionGroups.Update.NameEnAlreadyExists",
                    Message: ErrorMessage.UpdateQuestionGroup_NameEn_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            group.Update(
                nameEn: request.NameEn,
                nameAr: request.NameAr);

            _questionGroupWriteRepository.Update(group);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new UpdateQuestionGroupResponse
            {
                GroupId = group.Id,
                BranchId = group.BranchId,
                NameEn = group.NameEn,
                NameAr = group.NameAr,
                IsActive = group.IsActive
            };

            return Result<UpdateQuestionGroupResponse>.Ok(response);
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
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

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.UpdateGlobalQuestionGroup
{
    internal sealed class UpdateGlobalQuestionGroupCommandHandler
         : ICommandHandler<UpdateGlobalQuestionGroupCommand, UpdateGlobalQuestionGroupResponse>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteRepository<QuestionGroup> _questionGroupWriteRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateGlobalQuestionGroupCommandHandler(
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteRepository<QuestionGroup> questionGroupWriteRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _questionGroupWriteRepository = questionGroupWriteRepository
                ?? throw new ArgumentNullException(nameof(questionGroupWriteRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<UpdateGlobalQuestionGroupResponse>> Handle(
            UpdateGlobalQuestionGroupCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<UpdateGlobalQuestionGroupResponse>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Update.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForUpdateGlobalQuestionGroupSpec(
                    currentApplicationUserId),
                cancellationToken);

            if (currentSuperAdmin is null)
            {
                return Result<UpdateGlobalQuestionGroupResponse>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Update.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.UpdateGlobalQuestionGroup_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.Security));
            }

            var group = await _questionGroupReadRepository.FirstOrDefaultAsync(
                new GetGlobalQuestionGroupForUpdateSpec(request.GroupId),
                cancellationToken);

            if (group is null)
            {
                return Result<UpdateGlobalQuestionGroupResponse>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Update.QuestionGroupNotFound",
                    Message: ErrorMessage.UpdateGlobalQuestionGroup_QuestionGroup_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedNameEn = request.NameEn.Trim();

            var nameEnExists = await _questionGroupReadRepository.AnyAsync(
                x =>
                    x.Id != request.GroupId &&
                    x.Scope == QuestionScope.Global &&
                    x.BranchId == null &&
                    x.NameEn == normalizedNameEn,
                cancellationToken);

            if (nameEnExists)
            {
                return Result<UpdateGlobalQuestionGroupResponse>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Update.NameEnAlreadyExists",
                    Message: ErrorMessage.UpdateGlobalQuestionGroup_NameEn_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            group.Update(
                nameEn: request.NameEn,
                nameAr: request.NameAr);

            _questionGroupWriteRepository.Update(group);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new UpdateGlobalQuestionGroupResponse
            {
                GroupId = group.Id,
                BranchId = group.BranchId,
                Scope = group.Scope,
                ScopeName = group.Scope.ToString(),
                IsGlobal = group.Scope == QuestionScope.Global,
                IsEditable = group.Scope == QuestionScope.Global,
                NameEn = group.NameEn,
                NameAr = group.NameAr,
                IsActive = group.IsActive
            };

            return Result<UpdateGlobalQuestionGroupResponse>.Ok(response);
        }
    }
}
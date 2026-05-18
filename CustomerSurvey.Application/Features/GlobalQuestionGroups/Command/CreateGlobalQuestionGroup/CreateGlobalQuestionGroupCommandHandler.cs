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

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.CreateGlobalQuestionGroup
{
    internal sealed class CreateGlobalQuestionGroupCommandHandler
       : ICommandHandler<CreateGlobalQuestionGroupCommand, CreateGlobalQuestionGroupResponse>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteRepository<QuestionGroup> _questionGroupWriteRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateGlobalQuestionGroupCommandHandler(
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

        public async Task<Result<CreateGlobalQuestionGroupResponse>> Handle(
            CreateGlobalQuestionGroupCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateGlobalQuestionGroupResponse>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForCreateGlobalQuestionGroupSpec(
                    currentApplicationUserId),
                cancellationToken);

            if (currentSuperAdmin is null)
            {
                return Result<CreateGlobalQuestionGroupResponse>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Create.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.CreateGlobalQuestionGroup_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.Security));
            }

            var normalizedNameEn = request.NameEn.Trim();

            var nameEnExists = await _questionGroupReadRepository.AnyAsync(
                x =>
                    x.Scope == QuestionScope.Global &&
                    x.BranchId == null &&
                    x.NameEn == normalizedNameEn,
                cancellationToken);

            if (nameEnExists)
            {
                return Result<CreateGlobalQuestionGroupResponse>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Create.NameEnAlreadyExists",
                    Message: ErrorMessage.CreateGlobalQuestionGroup_NameEn_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var group = QuestionGroup.CreateGlobal(
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                createdByApplicationUserId: currentApplicationUserId);

            await _questionGroupWriteRepository.AddAsync(group, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateGlobalQuestionGroupResponse
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

            return Result<CreateGlobalQuestionGroupResponse>.Ok(response);
        }
    }
}
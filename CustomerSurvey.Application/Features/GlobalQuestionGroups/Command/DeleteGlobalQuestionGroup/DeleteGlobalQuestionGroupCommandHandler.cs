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

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.DeleteGlobalQuestionGroup
{
    internal sealed class DeleteGlobalQuestionGroupCommandHandler
        : ICommandHandler<DeleteGlobalQuestionGroupCommand, DeleteGlobalQuestionGroupResponse>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteRepository<QuestionGroup> _questionGroupWriteRepository;
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteGlobalQuestionGroupCommandHandler(
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteRepository<QuestionGroup> questionGroupWriteRepository,
            IWriteReadRepository<Question> questionReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _questionGroupWriteRepository = questionGroupWriteRepository
                ?? throw new ArgumentNullException(nameof(questionGroupWriteRepository));

            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<DeleteGlobalQuestionGroupResponse>> Handle(
            DeleteGlobalQuestionGroupCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<DeleteGlobalQuestionGroupResponse>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Delete.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForDeleteGlobalQuestionGroupSpec(
                    currentApplicationUserId),
                cancellationToken);

            if (currentSuperAdmin is null)
            {
                return Result<DeleteGlobalQuestionGroupResponse>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Delete.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.DeleteGlobalQuestionGroup_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.Security));
            }

            var group = await _questionGroupReadRepository.FirstOrDefaultAsync(
                new GetGlobalQuestionGroupForDeleteSpec(request.GroupId),
                cancellationToken);

            if (group is null)
            {
                return Result<DeleteGlobalQuestionGroupResponse>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Delete.QuestionGroupNotFound",
                    Message: ErrorMessage.DeleteGlobalQuestionGroup_QuestionGroup_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!group.IsActive)
            {
                return Result<DeleteGlobalQuestionGroupResponse>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Delete.QuestionGroupAlreadyInactive",
                    Message: ErrorMessage.DeleteGlobalQuestionGroup_QuestionGroup_AlreadyInactive,
                    Type: ErrorType.Validation));
            }

            var hasQuestions = await _questionReadRepository.AnyAsync(
                x =>
                    x.GroupId == group.Id &&
                    x.Scope == QuestionScope.Global,
                cancellationToken);

            if (hasQuestions)
            {
                return Result<DeleteGlobalQuestionGroupResponse>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Delete.QuestionGroupHasQuestions",
                    Message: ErrorMessage.DeleteGlobalQuestionGroup_QuestionGroup_HasQuestions,
                    Type: ErrorType.Validation));
            }

            group.Deactivate();

            _questionGroupWriteRepository.Update(group);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new DeleteGlobalQuestionGroupResponse
            {
                GroupId = group.Id,
                BranchId = group.BranchId,
                Scope = group.Scope,
                ScopeName = group.Scope.ToString(),
                IsGlobal = group.Scope == QuestionScope.Global,
                IsEditable = group.Scope == QuestionScope.Global,
                IsActive = group.IsActive
            };

            return Result<DeleteGlobalQuestionGroupResponse>.Ok(response);
        }
    }
}
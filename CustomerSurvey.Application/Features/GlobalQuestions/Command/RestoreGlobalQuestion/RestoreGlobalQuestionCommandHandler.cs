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

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.RestoreGlobalQuestion
{
    internal sealed class RestoreGlobalQuestionCommandHandler
        : ICommandHandler<RestoreGlobalQuestionCommand, RestoreGlobalQuestionResponse>
    {
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteRepository<Question> _questionWriteRepository;
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreGlobalQuestionCommandHandler(
            IWriteReadRepository<Question> questionReadRepository,
            IWriteRepository<Question> questionWriteRepository,
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _questionWriteRepository = questionWriteRepository
                ?? throw new ArgumentNullException(nameof(questionWriteRepository));

            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<RestoreGlobalQuestionResponse>> Handle(
            RestoreGlobalQuestionCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<RestoreGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Restore.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForRestoreGlobalQuestionSpec(
                    currentApplicationUserId),
                cancellationToken);

            if (currentSuperAdmin is null)
            {
                return Result<RestoreGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Restore.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.RestoreGlobalQuestion_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.Security));
            }

            var question = await _questionReadRepository.FirstOrDefaultAsync(
                new GetGlobalQuestionForRestoreSpec(request.QuestionId),
                cancellationToken);

            if (question is null)
            {
                return Result<RestoreGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Restore.QuestionNotFound",
                    Message: ErrorMessage.RestoreGlobalQuestion_Question_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (question.IsActive)
            {
                return Result<RestoreGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Restore.QuestionAlreadyActive",
                    Message: ErrorMessage.RestoreGlobalQuestion_Question_AlreadyActive,
                    Type: ErrorType.Validation));
            }

            var group = await _questionGroupReadRepository.FirstOrDefaultAsync(
                new GetGlobalQuestionGroupForRestoreGlobalQuestionSpec(question.GroupId),
                cancellationToken);

            if (group is null)
            {
                return Result<RestoreGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Restore.QuestionGroupNotFound",
                    Message: ErrorMessage.RestoreGlobalQuestion_QuestionGroup_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!group.IsActive)
            {
                return Result<RestoreGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Restore.QuestionGroupInactive",
                    Message: ErrorMessage.RestoreGlobalQuestion_QuestionGroup_Inactive,
                    Type: ErrorType.Validation));
            }

            question.Activate();

            _questionWriteRepository.Update(question);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new RestoreGlobalQuestionResponse
            {
                QuestionId = question.Id,
                BranchId = question.BranchId,
                GroupId = question.GroupId,
                GroupBranchId = group.BranchId,
                Scope = question.Scope,
                ScopeName = question.Scope.ToString(),
                IsGlobal = question.Scope == QuestionScope.Global,
                IsEditable = question.Scope == QuestionScope.Global,
                IsActive = question.IsActive
            };

            return Result<RestoreGlobalQuestionResponse>.Ok(response);
        }
    }
}
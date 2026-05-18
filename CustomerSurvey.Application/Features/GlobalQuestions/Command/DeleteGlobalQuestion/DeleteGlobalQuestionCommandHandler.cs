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

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.DeleteGlobalQuestion
{
    internal sealed class DeleteGlobalQuestionCommandHandler
          : ICommandHandler<DeleteGlobalQuestionCommand, DeleteGlobalQuestionResponse>
    {
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteRepository<Question> _questionWriteRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteGlobalQuestionCommandHandler(
            IWriteReadRepository<Question> questionReadRepository,
            IWriteRepository<Question> questionWriteRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _questionWriteRepository = questionWriteRepository
                ?? throw new ArgumentNullException(nameof(questionWriteRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<DeleteGlobalQuestionResponse>> Handle(
            DeleteGlobalQuestionCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<DeleteGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Delete.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForDeleteGlobalQuestionSpec(
                    currentApplicationUserId),
                cancellationToken);

            if (currentSuperAdmin is null)
            {
                return Result<DeleteGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Delete.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.DeleteGlobalQuestion_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.Security));
            }

            var question = await _questionReadRepository.FirstOrDefaultAsync(
                new GetGlobalQuestionForDeleteSpec(request.QuestionId),
                cancellationToken);

            if (question is null)
            {
                return Result<DeleteGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Delete.QuestionNotFound",
                    Message: ErrorMessage.DeleteGlobalQuestion_Question_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!question.IsActive)
            {
                return Result<DeleteGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Delete.QuestionAlreadyInactive",
                    Message: ErrorMessage.DeleteGlobalQuestion_Question_AlreadyInactive,
                    Type: ErrorType.Validation));
            }

            question.Deactivate();

            _questionWriteRepository.Update(question);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new DeleteGlobalQuestionResponse
            {
                QuestionId = question.Id,
                BranchId = question.BranchId,
                GroupId = question.GroupId,
                GroupBranchId = null,
                Scope = question.Scope,
                ScopeName = question.Scope.ToString(),
                IsGlobal = question.Scope == QuestionScope.Global,
                IsEditable = question.Scope == QuestionScope.Global,
                IsActive = question.IsActive
            };

            return Result<DeleteGlobalQuestionResponse>.Ok(response);
        }
    }
}
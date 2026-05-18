using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.CreateGlobalQuestion
{
    internal sealed class CreateGlobalQuestionCommandHandler
      : ICommandHandler<CreateGlobalQuestionCommand, CreateGlobalQuestionResponse>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteRepository<Question> _questionWriteRepository;
        private readonly IWriteRepository<QuestionOption> _questionOptionWriteRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateGlobalQuestionCommandHandler(
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteRepository<Question> questionWriteRepository,
            IWriteRepository<QuestionOption> questionOptionWriteRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _questionWriteRepository = questionWriteRepository
                ?? throw new ArgumentNullException(nameof(questionWriteRepository));

            _questionOptionWriteRepository = questionOptionWriteRepository
                ?? throw new ArgumentNullException(nameof(questionOptionWriteRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<CreateGlobalQuestionResponse>> Handle(
            CreateGlobalQuestionCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForCreateGlobalQuestionSpec(
                    currentApplicationUserId),
                cancellationToken);

            if (currentSuperAdmin is null)
            {
                return Result<CreateGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Create.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.CreateGlobalQuestion_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.Security));
            }

            var group = await _questionGroupReadRepository.FirstOrDefaultAsync(
                new GetGlobalQuestionGroupForCreateGlobalQuestionSpec(request.GroupId),
                cancellationToken);

            if (group is null)
            {
                return Result<CreateGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Create.QuestionGroupNotFound",
                    Message: ErrorMessage.CreateGlobalQuestion_Group_NotFound,
                    Type: ErrorType.NotFound));
            }

            var question = Question.CreateGlobal(
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
                    .OrderBy(x => x.Order)
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
                    .OrderBy(x => x.Order)
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

            var response = new CreateGlobalQuestionResponse
            {
                QuestionId = question.Id,
                BranchId = question.BranchId,
                GroupId = question.GroupId,
                GroupBranchId = group.BranchId,
                Scope = question.Scope,
                ScopeName = question.Scope.ToString(),
                IsGlobal = question.Scope == QuestionScope.Global,
                IsEditable = question.Scope == QuestionScope.Global,
                TextEn = question.TextEn,
                TextAr = question.TextAr,
                Type = question.Type,
                TypeName = question.Type.ToString(),
                IsActive = question.IsActive,
                Options = optionResponses
            };

            return Result<CreateGlobalQuestionResponse>.Ok(response);
        }
    }
}
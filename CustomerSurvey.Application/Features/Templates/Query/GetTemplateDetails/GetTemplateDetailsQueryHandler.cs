using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Templates.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateDetails
{
    internal sealed class GetTemplateDetailsQueryHandler
          : IQueryHandler<GetTemplateDetailsQuery, GetTemplateDetailsResponse>
    {
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<TemplateQuestionCondition> _conditionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetTemplateDetailsQueryHandler(
            IWriteReadRepository<Template> templateReadRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<TemplateQuestionCondition> conditionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            ICurrentUser currentUser)
        {
            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _templateQuestionReadRepository = templateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(templateQuestionReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _conditionReadRepository = conditionReadRepository
                ?? throw new ArgumentNullException(nameof(conditionReadRepository));
            _questionOptionReadRepository = questionOptionReadRepository
    ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));
        }

        public async Task<Result<GetTemplateDetailsResponse>> Handle(
            GetTemplateDetailsQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<GetTemplateDetailsResponse>.Fail(new Error(
                    Code: "Templates.Details.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorBranchIdResult = await ResolveCurrentActorBranchIdAsync(
                currentApplicationUserId,
                cancellationToken);

            if (actorBranchIdResult.IsFailure)
            {
                return Result<GetTemplateDetailsResponse>.Fail(actorBranchIdResult.Errors);
            }

            var branchId = actorBranchIdResult.Value;

            var template = await _templateReadRepository.FirstOrDefaultAsync(
                new GetTemplateBasicDetailsSpec(
                    templateId: request.TemplateId,
                    branchId: branchId),
                cancellationToken);

            if (template is null)
            {
                return Result<GetTemplateDetailsResponse>.Fail(new Error(
                    Code: "Templates.Details.TemplateNotFound",
                    Message: ErrorMessage.GetTemplateDetails_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            var templateQuestions = await _templateQuestionReadRepository.ListAsync(
                new GetTemplateQuestionsForTemplateDetailsSpec(request.TemplateId),
                cancellationToken);

            var groupsCount = templateQuestions
                .Select(x => x.GroupId)
                .Distinct()
                .Count();

            var questionConditions = await _conditionReadRepository.ListAsync(
    new GetTemplateQuestionConditionsByTemplateIdsSpec(
        new[] { request.TemplateId }),
    cancellationToken);
            var singleChoiceQuestionIds = templateQuestions
    .Where(x => x.Type == CustomerSurvey.Domain.Enums.QuestionType.SingleChoice)
    .Select(x => x.QuestionId)
    .Distinct()
    .ToArray();

            IReadOnlyCollection<CustomerSurvey.Application.Features.Questions.Shared.QuestionOptionResponse> questionOptions;

            if (singleChoiceQuestionIds.Length == 0)
            {
                questionOptions = Array.Empty<CustomerSurvey.Application.Features.Questions.Shared.QuestionOptionResponse>();
            }
            else
            {
                questionOptions = await _questionOptionReadRepository.ListAsync(
                    new CustomerSurvey.Application.Features.Questions.Shared.Specs.GetQuestionOptionsByQuestionIdsSpec(singleChoiceQuestionIds),
                    cancellationToken);
            }

            var optionsByQuestionId = questionOptions
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<TemplateDetailsQuestionOptionResponse>)x
                        .OrderBy(option => option.Order)
                        .Select(option => new TemplateDetailsQuestionOptionResponse
                        {
                            OptionId = option.OptionId,
                            TextEn = option.TextEn,
                            TextAr = option.TextAr,
                            Order = option.Order,
                            Value = option.Value
                        })
                        .ToArray());

            var response = new GetTemplateDetailsResponse
            {
                TemplateId = template.TemplateId,
                BranchId = template.BranchId,
                BranchNameEn = template.BranchNameEn,
                BranchNameAr = template.BranchNameAr,
                BranchCode = template.BranchCode,
                NameEn = template.NameEn,
                NameAr = template.NameAr,
                Description = template.Description,
                Status = template.Status.ToString(),
                IsActive = template.IsActive,
                CreatedOnUtc = template.CreatedOnUtc,
                ModifiedOnUtc = template.ModifiedOnUtc,
                QuestionConditions = questionConditions
    .OrderBy(x => x.Order)
    .Select(x => x.ToResponse())
    .ToArray(),

                Summary = new TemplateDetailsSummaryResponse
                {
                    QuestionsCount = templateQuestions.Count,
                    GroupsCount = groupsCount
                },

                Questions = templateQuestions
                    .Select(x => new TemplateDetailsQuestionResponse
                    {
                        TemplateQuestionId = x.TemplateQuestionId,
                        QuestionId = x.QuestionId,
                        Order = x.Order,
                        TextEn = x.TextEn,
                        TextAr = x.TextAr,
                        Type = x.Type.ToString(),
                        IsActive = x.IsActive,
                        GroupId = x.GroupId,
                        GroupNameEn = x.GroupNameEn,
                        GroupNameAr = x.GroupNameAr,
                        Options = x.Type == CustomerSurvey.Domain.Enums.QuestionType.SingleChoice
          && optionsByQuestionId.TryGetValue(x.QuestionId, out var options)
    ? options
    : Array.Empty<TemplateDetailsQuestionOptionResponse>()
                    })
                    .ToArray()
            };

            return Result<GetTemplateDetailsResponse>.Ok(response);
        }

        private async Task<Result<Guid>> ResolveCurrentActorBranchIdAsync(
            Guid currentApplicationUserId,
            CancellationToken cancellationToken)
        {
            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForTemplateDetailsSpec(currentApplicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return Result<Guid>.Ok(branchAdmin.BranchId);
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForTemplateDetailsSpec(currentApplicationUserId),
                cancellationToken);

            if (branchUser is not null)
            {
                return Result<Guid>.Ok(branchUser.BranchId);
            }

            return Result<Guid>.Fail(new Error(
                Code: "Templates.Details.CurrentBranchActorNotFound",
                Message: ErrorMessage.GetTemplateDetails_CurrentBranchActor_NotFound,
                Type: ErrorType.Security));
        }
    }
}
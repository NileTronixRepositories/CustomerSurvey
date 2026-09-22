using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorTemplatesSelection
{
    using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

    internal sealed class GetOperatorTemplatesSelectionQueryHandler
       : IQueryHandler<GetOperatorTemplatesSelectionQuery, GetOperatorTemplatesSelectionResponse>
    {
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
        private readonly IWriteReadRepository<DomainOperator> _operatorReadRepository;
        private readonly IWriteReadRepository<OperatorTemplate> _operatorTemplateReadRepository;
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetOperatorTemplatesSelectionQueryHandler(
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
            IWriteReadRepository<DomainOperator> operatorReadRepository,
            IWriteReadRepository<OperatorTemplate> operatorTemplateReadRepository,
            IWriteReadRepository<Template> templateReadRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
            ICurrentUser currentUser)
        {
            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _departmentAdminReadRepository = departmentAdminReadRepository
                ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));

            _operatorReadRepository = operatorReadRepository
                ?? throw new ArgumentNullException(nameof(operatorReadRepository));

            _operatorTemplateReadRepository = operatorTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(operatorTemplateReadRepository));

            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _templateQuestionReadRepository = templateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(templateQuestionReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<GetOperatorTemplatesSelectionResponse>> Handle(
            GetOperatorTemplatesSelectionQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<GetOperatorTemplatesSelectionResponse>.Fail(new Error(
                    Code: "Operators.TemplatesSelection.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorResult = await ResolveActorAsync(
                currentApplicationUserId,
                cancellationToken);

            if (actorResult.Error is not null)
            {
                return Result<GetOperatorTemplatesSelectionResponse>.Fail(actorResult.Error);
            }

            var operatorProfile = await _operatorReadRepository.FirstOrDefaultAsync(
                new GetOperatorForTemplatesSelectionSpec(request.OperatorId),
                cancellationToken);

            if (operatorProfile is null)
            {
                return Result<GetOperatorTemplatesSelectionResponse>.Fail(new Error(
                    Code: "Operators.TemplatesSelection.OperatorNotFound",
                    Message: ErrorMessage.GetOperatorTemplatesSelection_Operator_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (actorResult.DepartmentId.HasValue &&
                operatorProfile.DepartmentId != actorResult.DepartmentId.Value)
            {
                return Result<GetOperatorTemplatesSelectionResponse>.Fail(new Error(
                    Code: "Operators.TemplatesSelection.DepartmentScopeMismatch",
                    Message: ErrorMessage.GetOperatorTemplatesSelection_DepartmentScope_Mismatch,
                    Type: ErrorType.Security));
            }

            var assignedTemplateRows = await _operatorTemplateReadRepository.ListAsync(
                new GetAssignedTemplateIdsForOperatorSelectionSpec(operatorProfile.OperatorId),
                cancellationToken);

            var assignedTemplateIds = assignedTemplateRows
                .Select(x => x.TemplateId)
                .Distinct()
                .ToHashSet();

            var templates = await _templateReadRepository.ListAsync(
                new GetTemplatesForOperatorSelectionSpec(request.SearchText),
                cancellationToken);

            var templateIds = templates
                .Select(x => x.TemplateId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<TemplateQuestionForOperatorSelectionDto> templateQuestions;

            if (templateIds.Length == 0)
            {
                templateQuestions = Array.Empty<TemplateQuestionForOperatorSelectionDto>();
            }
            else
            {
                templateQuestions = await _templateQuestionReadRepository.ListAsync(
                    new GetTemplateQuestionsForOperatorSelectionSpec(templateIds),
                    cancellationToken);
            }

            var questionsByTemplateId = templateQuestions
                .GroupBy(x => x.TemplateId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<OperatorTemplateSelectionQuestionResponse>)x
                        .OrderBy(q => q.Order)
                        .Select(q => new OperatorTemplateSelectionQuestionResponse
                        {
                            TemplateQuestionId = q.TemplateQuestionId,
                            QuestionId = q.QuestionId,
                            Order = q.Order,
                            TextEn = q.TextEn,
                            TextAr = q.TextAr,
                            Type = q.Type,
                            IsActive = q.IsActive,
                            GroupId = q.GroupId,
                            GroupNameEn = q.GroupNameEn,
                            GroupNameAr = q.GroupNameAr
                        })
                        .ToArray());

            var templateItems = templates
                .Select(template =>
                {
                    var questions = questionsByTemplateId.TryGetValue(
                        template.TemplateId,
                        out var templateQuestionsList)
                            ? templateQuestionsList
                            : Array.Empty<OperatorTemplateSelectionQuestionResponse>();

                    return new OperatorTemplateSelectionItemResponse
                    {
                        TemplateId = template.TemplateId,
                        NameEn = template.NameEn,
                        NameAr = template.NameAr,
                        Description = template.Description,
                        BranchId = template.BranchId,
                        BranchNameEn = template.BranchNameEn,
                        BranchNameAr = template.BranchNameAr,
                        BranchCode = template.BranchCode,
                        LogoPath = template.LogoPath,
                        ActiveFrom = template.ActiveFrom,
                        ExpireTo = template.ExpireTo,
                        QuestionsCount = questions.Count,
                        Questions = questions
                    };
                })
                .ToArray();

            var selectedTemplates = templateItems
                .Where(x => assignedTemplateIds.Contains(x.TemplateId))
                .ToArray();

            var availableTemplates = templateItems
                .Where(x => !assignedTemplateIds.Contains(x.TemplateId))
                .ToArray();

            var response = new GetOperatorTemplatesSelectionResponse
            {
                OperatorId = operatorProfile.OperatorId,
                SelectedTemplatesCount = selectedTemplates.Length,
                AvailableTemplatesCount = availableTemplates.Length,
                SelectedTemplates = selectedTemplates,
                AvailableTemplates = availableTemplates
            };

            return Result<GetOperatorTemplatesSelectionResponse>.Ok(response);
        }

        private async Task<GetOperatorTemplatesSelectionActorResult> ResolveActorAsync(
            Guid currentApplicationUserId,
            CancellationToken cancellationToken)
        {
            var isSuperAdmin = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            var departmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentDepartmentAdminForOperatorTemplatesSelectionSpec(currentApplicationUserId),
                cancellationToken);

            var actorMatchCount =
                (isSuperAdmin ? 1 : 0) +
                (departmentAdmin is not null ? 1 : 0);

            if (actorMatchCount == 0)
            {
                return GetOperatorTemplatesSelectionActorResult.Fail(new Error(
                    Code: "Operators.TemplatesSelection.CurrentActorNotAllowed",
                    Message: ErrorMessage.GetOperatorTemplatesSelection_CurrentActor_NotAllowed,
                    Type: ErrorType.Security));
            }

            if (actorMatchCount > 1)
            {
                return GetOperatorTemplatesSelectionActorResult.Fail(new Error(
                    Code: "Operators.TemplatesSelection.ActorAmbiguous",
                    Message: ErrorMessage.GetOperatorTemplatesSelection_Actor_Ambiguous,
                    Type: ErrorType.Security));
            }

            if (isSuperAdmin)
            {
                return GetOperatorTemplatesSelectionActorResult.Ok(departmentId: null);
            }

            return GetOperatorTemplatesSelectionActorResult.Ok(
                departmentId: departmentAdmin!.DepartmentId);
        }

        private sealed record GetOperatorTemplatesSelectionActorResult
        {
            public Guid? DepartmentId { get; init; }

            public Error? Error { get; init; }

            public static GetOperatorTemplatesSelectionActorResult Ok(Guid? departmentId)
            {
                return new GetOperatorTemplatesSelectionActorResult
                {
                    DepartmentId = departmentId
                };
            }

            public static GetOperatorTemplatesSelectionActorResult Fail(Error error)
            {
                return new GetOperatorTemplatesSelectionActorResult
                {
                    Error = error
                };
            }
        }
    }
}

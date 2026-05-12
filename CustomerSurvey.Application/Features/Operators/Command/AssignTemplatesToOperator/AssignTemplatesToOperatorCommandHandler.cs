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

namespace CustomerSurvey.Application.Features.Operators.Command.AssignTemplatesToOperator
{
    using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

    internal sealed class AssignTemplatesToOperatorCommandHandler
        : ICommandHandler<AssignTemplatesToOperatorCommand, AssignTemplatesToOperatorResponse>
    {
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
        private readonly IWriteReadRepository<DomainOperator> _operatorReadRepository;

        private readonly IWriteReadRepository<Template> _templateReadRepository;

        private readonly IWriteReadRepository<OperatorTemplate> _operatorTemplateReadRepository;
        private readonly IWriteRepository<OperatorTemplate> _operatorTemplateWriteRepository;

        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public AssignTemplatesToOperatorCommandHandler(
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
            IWriteReadRepository<DomainOperator> operatorReadRepository,
            IWriteReadRepository<Template> templateReadRepository,
            IWriteReadRepository<OperatorTemplate> operatorTemplateReadRepository,
            IWriteRepository<OperatorTemplate> operatorTemplateWriteRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _departmentAdminReadRepository = departmentAdminReadRepository
                ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));

            _operatorReadRepository = operatorReadRepository
                ?? throw new ArgumentNullException(nameof(operatorReadRepository));

            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _operatorTemplateReadRepository = operatorTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(operatorTemplateReadRepository));

            _operatorTemplateWriteRepository = operatorTemplateWriteRepository
                ?? throw new ArgumentNullException(nameof(operatorTemplateWriteRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<AssignTemplatesToOperatorResponse>> Handle(
            AssignTemplatesToOperatorCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<AssignTemplatesToOperatorResponse>.Fail(new Error(
                    Code: "Operators.AssignTemplates.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorResult = await ResolveActorAsync(
                currentApplicationUserId,
                cancellationToken);

            if (actorResult.Error is not null)
            {
                return Result<AssignTemplatesToOperatorResponse>.Fail(actorResult.Error);
            }

            var operatorProfile = await _operatorReadRepository.FirstOrDefaultAsync(
                new GetOperatorForAssignTemplatesToOperatorSpec(request.OperatorId),
                cancellationToken);

            if (operatorProfile is null)
            {
                return Result<AssignTemplatesToOperatorResponse>.Fail(new Error(
                    Code: "Operators.AssignTemplates.OperatorNotFound",
                    Message: ErrorMessage.AssignTemplatesToOperator_Operator_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (actorResult.DepartmentId.HasValue &&
                operatorProfile.DepartmentId != actorResult.DepartmentId.Value)
            {
                return Result<AssignTemplatesToOperatorResponse>.Fail(new Error(
                    Code: "Operators.AssignTemplates.DepartmentScopeMismatch",
                    Message: ErrorMessage.AssignTemplatesToOperator_DepartmentScope_Mismatch,
                    Type: ErrorType.Security));
            }

            var requestedTemplateIds = request.TemplateIds
                .Distinct()
                .ToArray();

            if (requestedTemplateIds.Length > 0)
            {
                var activeTemplates = await _templateReadRepository.ListAsync(
                    new GetActiveTemplatesForAssignTemplatesToOperatorSpec(requestedTemplateIds),
                    cancellationToken);

                var activeTemplateIds = activeTemplates
                    .Select(x => x.TemplateId)
                    .ToHashSet();

                var hasMissingOrInactiveTemplate = requestedTemplateIds
                    .Any(templateId => !activeTemplateIds.Contains(templateId));

                if (hasMissingOrInactiveTemplate)
                {
                    return Result<AssignTemplatesToOperatorResponse>.Fail(new Error(
                        Code: "Operators.AssignTemplates.TemplateNotFoundOrInactive",
                        Message: ErrorMessage.AssignTemplatesToOperator_Template_NotFoundOrInactive,
                        Type: ErrorType.Validation));
                }
            }

            var currentAssignments = await _operatorTemplateReadRepository.ListAsync(
                new GetCurrentOperatorTemplatesForAssignTemplatesToOperatorSpec(operatorProfile.OperatorId),
                cancellationToken);

            var requestedTemplateIdSet = requestedTemplateIds.ToHashSet();

            var currentTemplateIdSet = currentAssignments
                .Select(x => x.TemplateId)
                .ToHashSet();

            var assignmentsToRemove = currentAssignments
                .Where(x => !requestedTemplateIdSet.Contains(x.TemplateId))
                .ToArray();

            var templateIdsToAdd = requestedTemplateIds
                .Where(templateId => !currentTemplateIdSet.Contains(templateId))
                .ToArray();

            foreach (var assignment in assignmentsToRemove)
            {
                _operatorTemplateWriteRepository.Delete(assignment);
            }

            foreach (var templateId in templateIdsToAdd)
            {
                var operatorTemplate = OperatorTemplate.Create(
                    operatorId: operatorProfile.OperatorId,
                    templateId: templateId,
                    createdByApplicationUserId: currentApplicationUserId);

                await _operatorTemplateWriteRepository.AddAsync(
                    operatorTemplate,
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new AssignTemplatesToOperatorResponse
            {
                OperatorId = operatorProfile.OperatorId,
                AssignedTemplatesCount = requestedTemplateIds.Length
            };

            return Result<AssignTemplatesToOperatorResponse>.Ok(response);
        }

        private async Task<AssignTemplatesToOperatorActorResult> ResolveActorAsync(
            Guid currentApplicationUserId,
            CancellationToken cancellationToken)
        {
            var isSuperAdmin = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            var departmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentDepartmentAdminForAssignTemplatesToOperatorSpec(currentApplicationUserId),
                cancellationToken);

            var actorMatchCount =
                (isSuperAdmin ? 1 : 0) +
                (departmentAdmin is not null ? 1 : 0);

            if (actorMatchCount == 0)
            {
                return AssignTemplatesToOperatorActorResult.Fail(new Error(
                    Code: "Operators.AssignTemplates.CurrentActorNotAllowed",
                    Message: ErrorMessage.AssignTemplatesToOperator_CurrentActor_NotAllowed,
                    Type: ErrorType.Security));
            }

            if (actorMatchCount > 1)
            {
                return AssignTemplatesToOperatorActorResult.Fail(new Error(
                    Code: "Operators.AssignTemplates.ActorAmbiguous",
                    Message: ErrorMessage.AssignTemplatesToOperator_Actor_Ambiguous,
                    Type: ErrorType.Security));
            }

            if (isSuperAdmin)
            {
                return AssignTemplatesToOperatorActorResult.Ok(departmentId: null);
            }

            return AssignTemplatesToOperatorActorResult.Ok(
                departmentId: departmentAdmin!.DepartmentId);
        }

        private sealed record AssignTemplatesToOperatorActorResult
        {
            public Guid? DepartmentId { get; init; }

            public Error? Error { get; init; }

            public static AssignTemplatesToOperatorActorResult Ok(Guid? departmentId)
            {
                return new AssignTemplatesToOperatorActorResult
                {
                    DepartmentId = departmentId
                };
            }

            public static AssignTemplatesToOperatorActorResult Fail(Error error)
            {
                return new AssignTemplatesToOperatorActorResult
                {
                    Error = error
                };
            }
        }
    }
}
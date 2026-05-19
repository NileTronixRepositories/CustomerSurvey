using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Templates.Command.UpdateTemplate
{
    internal sealed class UpdateTemplateCommandHandler
        : ICommandHandler<UpdateTemplateCommand, UpdateTemplateResponse>
    {
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteRepository<Template> _templateWriteRepository;

        private readonly IWriteReadRepository<TemplateCustomInput> _templateCustomInputReadRepository;
        private readonly IWriteRepository<TemplateCustomInput> _templateCustomInputWriteRepository;

        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;

        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTemplateCommandHandler(
            IWriteReadRepository<Template> templateReadRepository,
            IWriteRepository<Template> templateWriteRepository,
            IWriteReadRepository<TemplateCustomInput> templateCustomInputReadRepository,
            IWriteRepository<TemplateCustomInput> templateCustomInputWriteRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _templateWriteRepository = templateWriteRepository
                ?? throw new ArgumentNullException(nameof(templateWriteRepository));

            _templateCustomInputReadRepository = templateCustomInputReadRepository
                ?? throw new ArgumentNullException(nameof(templateCustomInputReadRepository));

            _templateCustomInputWriteRepository = templateCustomInputWriteRepository
                ?? throw new ArgumentNullException(nameof(templateCustomInputWriteRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<UpdateTemplateResponse>> Handle(
            UpdateTemplateCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<UpdateTemplateResponse>.Fail(new Error(
                    Code: "Templates.Update.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorBranchIdResult = await ResolveCurrentActorBranchIdAsync(
                currentApplicationUserId,
                cancellationToken);

            if (actorBranchIdResult.IsFailure)
            {
                return Result<UpdateTemplateResponse>.Fail(actorBranchIdResult.Errors);
            }

            var branchId = actorBranchIdResult.Value;

            var template = await _templateReadRepository.FirstOrDefaultAsync(
                new GetTemplateForUpdateSpec(
                    templateId: request.TemplateId,
                    branchId: branchId),
                cancellationToken);

            if (template is null)
            {
                return Result<UpdateTemplateResponse>.Fail(new Error(
                    Code: "Templates.Update.TemplateNotFound",
                    Message: ErrorMessage.UpdateTemplate_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedNameEn = request.NameEn.Trim();

            var templateNameExists = await _templateReadRepository.AnyAsync(
                x => x.Id != request.TemplateId &&
                     x.BranchId == branchId &&
                     x.NameEn == normalizedNameEn,
                cancellationToken);

            if (templateNameExists)
            {
                return Result<UpdateTemplateResponse>.Fail(new Error(
                    Code: "Templates.Update.NameEnAlreadyExistsInsideBranch",
                    Message: ErrorMessage.UpdateTemplate_NameEn_AlreadyExists_InsideBranch,
                    Type: ErrorType.Validation));
            }

            var existingCustomInputs = await _templateCustomInputReadRepository.ListAsync(
                new GetTemplateCustomInputsForUpdateSpec(request.TemplateId),
                cancellationToken);

            template.Update(
                nameEn: normalizedNameEn,
                nameAr: request.NameAr,
                description: request.Description,
                activeFrom: request.ActiveFrom,
                expireTo: request.ExpireTo);

            var customInputsUpdateResult = ApplyCustomInputsUpdate(
                templateId: template.Id,
                existingCustomInputs: existingCustomInputs,
                requestedCustomInputs: request.CustomInputs,
                currentApplicationUserId: currentApplicationUserId);

            if (customInputsUpdateResult.Error is not null)
            {
                return Result<UpdateTemplateResponse>.Fail(customInputsUpdateResult.Error);
            }

            _templateWriteRepository.Update(template);

            foreach (var customInputToUpdate in customInputsUpdateResult.CustomInputsToUpdate)
            {
                _templateCustomInputWriteRepository.Update(customInputToUpdate);
            }

            foreach (var newCustomInput in customInputsUpdateResult.NewCustomInputs)
            {
                await _templateCustomInputWriteRepository.AddAsync(
                    newCustomInput,
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var activeCustomInputs = customInputsUpdateResult.CustomInputsToUpdate
                .Where(x => x.IsActive)
                .Concat(customInputsUpdateResult.NewCustomInputs)
                .OrderBy(x => x.Order)
                .ToArray();

            var response = new UpdateTemplateResponse
            {
                TemplateId = template.Id,
                BranchId = template.BranchId,
                NameEn = template.NameEn,
                NameAr = template.NameAr,
                Description = template.Description,
                ActiveFrom = template.ActiveFrom,
                ExpireTo = template.ExpireTo,
                Status = template.Status.ToString(),
                IsActive = template.IsActive,

                CustomInputs = activeCustomInputs
                    .Select(x => new UpdateTemplateCustomInputResponse
                    {
                        CustomInputId = x.Id,
                        Name = x.Name,
                        LabelEn = x.LabelEn,
                        LabelAr = x.LabelAr,
                        Type = x.Type,
                        IsRequired = x.IsRequired,
                        MinLength = x.MinLength,
                        MaxLength = x.MaxLength,
                        MinValue = x.MinValue,
                        MaxValue = x.MaxValue,
                        Order = x.Order,
                        IsActive = x.IsActive
                    })
                    .ToArray()
            };

            return Result<UpdateTemplateResponse>.Ok(response);
        }

        private async Task<Result<Guid>> ResolveCurrentActorBranchIdAsync(
            Guid currentApplicationUserId,
            CancellationToken cancellationToken)
        {
            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForUpdateTemplateSpec(currentApplicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return Result<Guid>.Ok(branchAdmin.BranchId);
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForUpdateTemplateSpec(currentApplicationUserId),
                cancellationToken);

            if (branchUser is not null)
            {
                return Result<Guid>.Ok(branchUser.BranchId);
            }

            return Result<Guid>.Fail(new Error(
                Code: "Templates.Update.CurrentBranchActorNotFound",
                Message: ErrorMessage.UpdateTemplate_CurrentBranchActor_NotFound,
                Type: ErrorType.Security));
        }

        private static ApplyCustomInputsUpdateResult ApplyCustomInputsUpdate(
            Guid templateId,
            IReadOnlyCollection<TemplateCustomInput> existingCustomInputs,
            IReadOnlyCollection<UpdateTemplateCustomInputCommandItem> requestedCustomInputs,
            Guid currentApplicationUserId)
        {
            requestedCustomInputs ??= Array.Empty<UpdateTemplateCustomInputCommandItem>();

            var existingCustomInputsList = existingCustomInputs.ToList();

            var finalActiveCustomInputIds = new HashSet<Guid>();
            var newCustomInputs = new List<TemplateCustomInput>();

            foreach (var requestedCustomInput in requestedCustomInputs.OrderBy(x => x.Order))
            {
                var normalizedName = requestedCustomInput.Name.Trim();

                TemplateCustomInput? customInput;

                if (requestedCustomInput.CustomInputId.HasValue)
                {
                    customInput = existingCustomInputsList.FirstOrDefault(
                        x => x.Id == requestedCustomInput.CustomInputId.Value);

                    if (customInput is null)
                    {
                        return ApplyCustomInputsUpdateResult.Fail(new Error(
                            Code: "Templates.Update.CustomInputNotFound",
                            Message: ErrorMessage.UpdateTemplate_CustomInput_NotFound,
                            Type: ErrorType.Validation));
                    }
                }
                else
                {
                    customInput = existingCustomInputsList.FirstOrDefault(
                        x => string.Equals(
                            x.Name,
                            normalizedName,
                            StringComparison.OrdinalIgnoreCase));
                }

                if (customInput is not null)
                {
                    if (customInput.Type != requestedCustomInput.Type)
                    {
                        return ApplyCustomInputsUpdateResult.Fail(new Error(
                            Code: "Templates.Update.CustomInputTypeCannotBeChanged",
                            Message: ErrorMessage.UpdateTemplate_CustomInput_Type_CannotBeChanged,
                            Type: ErrorType.Validation));
                    }

                    customInput.Update(
                        name: requestedCustomInput.Name,
                        labelEn: requestedCustomInput.LabelEn,
                        labelAr: requestedCustomInput.LabelAr,
                        type: requestedCustomInput.Type,
                        isRequired: requestedCustomInput.IsRequired,
                        minLength: requestedCustomInput.MinLength,
                        maxLength: requestedCustomInput.MaxLength,
                        minValue: requestedCustomInput.MinValue,
                        maxValue: requestedCustomInput.MaxValue,
                        order: requestedCustomInput.Order);

                    customInput.Restore();

                    finalActiveCustomInputIds.Add(customInput.Id);
                    continue;
                }

                var newCustomInput = TemplateCustomInput.Create(
                    templateId: templateId,
                    name: requestedCustomInput.Name,
                    labelEn: requestedCustomInput.LabelEn,
                    labelAr: requestedCustomInput.LabelAr,
                    type: requestedCustomInput.Type,
                    isRequired: requestedCustomInput.IsRequired,
                    minLength: requestedCustomInput.MinLength,
                    maxLength: requestedCustomInput.MaxLength,
                    minValue: requestedCustomInput.MinValue,
                    maxValue: requestedCustomInput.MaxValue,
                    order: requestedCustomInput.Order,
                    createdByApplicationUserId: currentApplicationUserId);

                newCustomInputs.Add(newCustomInput);
                finalActiveCustomInputIds.Add(newCustomInput.Id);
            }

            foreach (var existingCustomInput in existingCustomInputsList)
            {
                if (existingCustomInput.IsActive &&
                    !finalActiveCustomInputIds.Contains(existingCustomInput.Id))
                {
                    existingCustomInput.Deactivate();
                }
            }

            return ApplyCustomInputsUpdateResult.Ok(
                customInputsToUpdate: existingCustomInputsList,
                newCustomInputs: newCustomInputs);
        }

        private sealed record ApplyCustomInputsUpdateResult(
            Error? Error,
            IReadOnlyCollection<TemplateCustomInput> CustomInputsToUpdate,
            IReadOnlyCollection<TemplateCustomInput> NewCustomInputs)
        {
            public static ApplyCustomInputsUpdateResult Ok(
                IReadOnlyCollection<TemplateCustomInput> customInputsToUpdate,
                IReadOnlyCollection<TemplateCustomInput> newCustomInputs)
            {
                return new ApplyCustomInputsUpdateResult(
                    Error: null,
                    CustomInputsToUpdate: customInputsToUpdate,
                    NewCustomInputs: newCustomInputs);
            }

            public static ApplyCustomInputsUpdateResult Fail(Error error)
            {
                return new ApplyCustomInputsUpdateResult(
                    Error: error,
                    CustomInputsToUpdate: Array.Empty<TemplateCustomInput>(),
                    NewCustomInputs: Array.Empty<TemplateCustomInput>());
            }
        }
    }
}
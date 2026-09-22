using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
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
        private readonly IWriteReadRepository<Branch> _branchReadRepository;

        private readonly IWriteReadRepository<TemplateCustomInput> _templateCustomInputReadRepository;
        private readonly IWriteRepository<TemplateCustomInput> _templateCustomInputWriteRepository;

        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTemplateCommandHandler(
            IWriteReadRepository<Template> templateReadRepository,
            IWriteRepository<Template> templateWriteRepository,
            IWriteReadRepository<Branch> branchReadRepository,
            IWriteReadRepository<TemplateCustomInput> templateCustomInputReadRepository,
            IWriteRepository<TemplateCustomInput> templateCustomInputWriteRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _templateWriteRepository = templateWriteRepository
                ?? throw new ArgumentNullException(nameof(templateWriteRepository));

            _branchReadRepository = branchReadRepository
                ?? throw new ArgumentNullException(nameof(branchReadRepository));

            _templateCustomInputReadRepository = templateCustomInputReadRepository
                ?? throw new ArgumentNullException(nameof(templateCustomInputReadRepository));

            _templateCustomInputWriteRepository = templateCustomInputWriteRepository
                ?? throw new ArgumentNullException(nameof(templateCustomInputWriteRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

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

            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                cancellationToken);

            if (currentBranchScope.IsFailure)
            {
                return Result<UpdateTemplateResponse>.Fail(currentBranchScope.Errors);
            }

            var branchId = currentBranchScope.Value.BranchId;

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

            var branch = await _branchReadRepository.GetByPropertyAsync(
                x => x.Id == branchId,
                cancellationToken);

            var response = new UpdateTemplateResponse
            {
                TemplateId = template.Id,
                BranchId = template.BranchId,
                BranchNameEn = branch?.NameEn ?? string.Empty,
                BranchNameAr = branch?.NameAr,
                NameEn = template.NameEn,
                NameAr = template.NameAr,
                Description = template.Description,
                ActiveFrom = template.ActiveFrom,
                ExpireTo = template.ExpireTo,
                IsActive = template.IsActive,
                LogoPath = template.LogoPath,

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
                        StartWith = x.StartWith,
                        Order = x.Order,
                        IsActive = x.IsActive
                    })
                    .ToArray()
            };

            return Result<UpdateTemplateResponse>.Ok(response);
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
                        startWith: requestedCustomInput.StartWith,
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
                    startWith: requestedCustomInput.StartWith,
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

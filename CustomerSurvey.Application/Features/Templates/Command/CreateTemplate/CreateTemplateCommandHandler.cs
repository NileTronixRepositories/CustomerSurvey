using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.CreateTemplate
{
    internal sealed class CreateTemplateCommandHandler
         : ICommandHandler<CreateTemplateCommand, CreateTemplateResponse>
    {
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteRepository<Template> _templateWriteRepository;

        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTemplateCommandHandler(
            IWriteReadRepository<Template> templateReadRepository,
            IWriteRepository<Template> templateWriteRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _templateWriteRepository = templateWriteRepository
                ?? throw new ArgumentNullException(nameof(templateWriteRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<CreateTemplateResponse>> Handle(
            CreateTemplateCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateTemplateResponse>.Fail(new Error(
                    Code: "Templates.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                cancellationToken);

            if (currentBranchScope.IsFailure)
            {
                return Result<CreateTemplateResponse>.Fail(currentBranchScope.Errors);
            }

            var branchId = currentBranchScope.Value.BranchId;

            var normalizedNameEn = request.NameEn.Trim();

            var templateNameExists = await _templateReadRepository.AnyAsync(
                x => x.BranchId == branchId && x.NameEn == normalizedNameEn,
                cancellationToken);

            if (templateNameExists)
            {
                return Result<CreateTemplateResponse>.Fail(new Error(
                    Code: "Templates.Create.NameEnAlreadyExistsInsideBranch",
                    Message: ErrorMessage.CreateTemplate_NameEn_AlreadyExists_InsideBranch,
                    Type: ErrorType.Validation));
            }

            var template = Template.Create(
                branchId: branchId,
                nameEn: normalizedNameEn,
                nameAr: request.NameAr,
                description: request.Description,
                activeFrom: request.ActiveFrom,
                expireTo: request.ExpireTo,
                createdByApplicationUserId: currentApplicationUserId);

            foreach (var customInputRequest in request.CustomInputs.OrderBy(x => x.Order))
            {
                var customInput = TemplateCustomInput.Create(
                    templateId: template.Id,
                    name: customInputRequest.Name,
                    labelEn: customInputRequest.LabelEn,
                    labelAr: customInputRequest.LabelAr,
                    type: customInputRequest.Type,
                    isRequired: customInputRequest.IsRequired,
                    minLength: customInputRequest.MinLength,
                    maxLength: customInputRequest.MaxLength,
                    minValue: customInputRequest.MinValue,
                    maxValue: customInputRequest.MaxValue,
                    startWith: customInputRequest.StartWith,
                    order: customInputRequest.Order,
                    createdByApplicationUserId: currentApplicationUserId);

                template.AddCustomInput(customInput);
            }

            await _templateWriteRepository.AddAsync(template, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateTemplateResponse
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
                CustomInputs = template.CustomInputs
                    .OrderBy(x => x.Order)
                    .Select(x => new CreateTemplateCustomInputResponse
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

            return Result<CreateTemplateResponse>.Ok(response);
        }

    }
}

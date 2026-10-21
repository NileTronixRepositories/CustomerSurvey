using CustomerSurvey.Domain.Resources;
using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Branches.EvaluationSettings;

public sealed record GetBranchEvaluationSettingsQuery(Guid BranchId) : IQuery<BranchEvaluationSettingsResponse>;
public sealed record UpdateBranchEvaluationSettingsCommand : ICommand<BranchEvaluationSettingsResponse>
{
    public Guid BranchId { get; init; }
    public decimal NeutralMinimum { get; init; }
    public decimal SatisfiedMinimum { get; init; }
    public decimal MediumRiskMinimum { get; init; }
    public decimal HealthyMinimum { get; init; }
}
public sealed record BranchEvaluationSettingsResponse(Guid BranchId, decimal NeutralMinimum, decimal SatisfiedMinimum,
    decimal MediumRiskMinimum, decimal HealthyMinimum, DateTime? UpdatedOnUtc, bool UsesDefaults);

internal sealed class UpdateBranchEvaluationSettingsValidator : AbstractValidator<UpdateBranchEvaluationSettingsCommand>
{
    public UpdateBranchEvaluationSettingsValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x).Must(x => BranchEvaluationSettings.IsValid(x.NeutralMinimum, x.SatisfiedMinimum,
            x.MediumRiskMinimum, x.HealthyMinimum)).WithMessage(ErrorMessage.BranchEvaluationSettings_InvalidThresholds);
    }
}

internal sealed class BranchEvaluationSettingsHandlers(
    ICurrentBranchScopeResolver scopeResolver, ICurrentUser currentUser,
    IWriteReadRepository<BranchEvaluationSettings> read, IWriteRepository<BranchEvaluationSettings> write,
    IUnitOfWork unitOfWork)
    : IQueryHandler<GetBranchEvaluationSettingsQuery, BranchEvaluationSettingsResponse>,
      ICommandHandler<UpdateBranchEvaluationSettingsCommand, BranchEvaluationSettingsResponse>
{
    public async Task<Result<BranchEvaluationSettingsResponse>> Handle(GetBranchEvaluationSettingsQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue) return Denied();
        var scope = await scopeResolver.ResolveAsync(ct);
        if (scope.IsFailure) return Result<BranchEvaluationSettingsResponse>.Fail(scope.Errors);
        if (scope.Value.BranchId != request.BranchId) return Denied();
        var settings = await read.FirstOrDefaultAsync(new SettingsByBranchSpec(request.BranchId), ct);
        return Result<BranchEvaluationSettingsResponse>.Ok(Map(request.BranchId, settings));
    }

    public async Task<Result<BranchEvaluationSettingsResponse>> Handle(UpdateBranchEvaluationSettingsCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue) return Denied();
        var scope = await scopeResolver.ResolveAsync(ct);
        if (scope.IsFailure) return Result<BranchEvaluationSettingsResponse>.Fail(scope.Errors);
        if (!currentUser.IsAuthenticated || currentUser.UserTypeValue != (int)CustomerSurvey.Domain.Enums.UserType.BranchAdmin ||
            scope.Value.ActorType != CustomerSurvey.Domain.Enums.UserType.BranchAdmin || scope.Value.BranchId != request.BranchId) return Denied();
        if (!BranchEvaluationSettings.IsValid(request.NeutralMinimum, request.SatisfiedMinimum, request.MediumRiskMinimum, request.HealthyMinimum))
            return Result<BranchEvaluationSettingsResponse>.Fail(new Error("BranchEvaluationSettings.InvalidThresholds",
                ErrorMessage.BranchEvaluationSettings_InvalidThresholds, ErrorType.Validation));
        var settings = await read.FirstOrDefaultAsync(new SettingsByBranchSpec(request.BranchId), ct);
        if (settings is null)
        {
            settings = BranchEvaluationSettings.Create(request.BranchId, currentUser.UserId!.Value);
            await write.AddAsync(settings, ct);
        }
        settings.Update(request.NeutralMinimum, request.SatisfiedMinimum, request.MediumRiskMinimum,
            request.HealthyMinimum, currentUser.UserId!.Value);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<BranchEvaluationSettingsResponse>.Ok(Map(request.BranchId, settings));
    }

    private static BranchEvaluationSettingsResponse Map(Guid branchId, BranchEvaluationSettings? x)
        => new(branchId, x?.NeutralMinimum ?? 60m, x?.SatisfiedMinimum ?? 80m, x?.MediumRiskMinimum ?? 50m,
            x?.HealthyMinimum ?? 75m, x?.UpdatedOnUtc, x is null);
    private static Result<BranchEvaluationSettingsResponse> Denied() => Result<BranchEvaluationSettingsResponse>.Fail(
        new Error("BranchEvaluationSettings.Forbidden", ErrorMessage.BranchEvaluationSettings_Forbidden, ErrorType.Security));
    private sealed class SettingsByBranchSpec : Specification<BranchEvaluationSettings>
    {
        public SettingsByBranchSpec(Guid branchId) => AddCriteria(x => x.BranchId == branchId);
    }
}

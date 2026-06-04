using CustomerSurvey.Application.Features.Auth.Command.SelectBranch;
using CustomerSurvey.Application.Features.BranchAreas.Command.AssignBranchesToBranchArea;
using CustomerSurvey.Application.Features.BranchAreas.Command.CreateBranchArea;
using CustomerSurvey.Domain.Resources;
using Xunit;

namespace CustomerSurvey.Tests.BranchAreas;

public sealed class BranchAreaValidationTests
{
    [Fact]
    public void SelectBranch_RejectsEmptyBranchId()
    {
        var validator = new SelectBranchCommandValidator();

        var result = validator.Validate(new SelectBranchCommand
        {
            BranchId = Guid.Empty
        });

        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == ErrorMessage.SelectBranch_BranchId_Required);
    }

    [Fact]
    public void CreateBranchArea_RejectsEmptyBranchIds()
    {
        var validator = new CreateBranchAreaCommandValidator();

        var result = validator.Validate(CreateValidCreateCommand() with
        {
            BranchIds = Array.Empty<Guid>()
        });

        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == ErrorMessage.CreateBranchArea_Branches_Required);
    }

    [Fact]
    public void CreateBranchArea_RejectsDuplicateBranchIds()
    {
        var validator = new CreateBranchAreaCommandValidator();
        var branchId = Guid.NewGuid();

        var result = validator.Validate(CreateValidCreateCommand() with
        {
            BranchIds = new[] { branchId, branchId }
        });

        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == ErrorMessage.CreateBranchArea_BranchIds_Duplicated);
    }

    [Fact]
    public void CreateBranchArea_RejectsEmptyBranchIdItem()
    {
        var validator = new CreateBranchAreaCommandValidator();

        var result = validator.Validate(CreateValidCreateCommand() with
        {
            BranchIds = new[] { Guid.Empty }
        });

        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == ErrorMessage.CreateBranchArea_BranchIds_Invalid);
    }

    [Fact]
    public void AssignBranches_RejectsEmptyBranchIds()
    {
        var validator = new AssignBranchesToBranchAreaCommandValidator();

        var result = validator.Validate(new AssignBranchesToBranchAreaCommand
        {
            BranchAreaId = Guid.NewGuid(),
            BranchIds = Array.Empty<Guid>()
        });

        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == ErrorMessage.AssignBranchAreaBranches_Branches_Required);
    }

    [Fact]
    public void AssignBranches_RejectsDuplicateBranchIds()
    {
        var validator = new AssignBranchesToBranchAreaCommandValidator();
        var branchId = Guid.NewGuid();

        var result = validator.Validate(new AssignBranchesToBranchAreaCommand
        {
            BranchAreaId = Guid.NewGuid(),
            BranchIds = new[] { branchId, branchId }
        });

        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == ErrorMessage.CreateBranchArea_BranchIds_Duplicated);
    }

    [Fact]
    public void AssignBranches_RejectsEmptyBranchIdItem()
    {
        var validator = new AssignBranchesToBranchAreaCommandValidator();

        var result = validator.Validate(new AssignBranchesToBranchAreaCommand
        {
            BranchAreaId = Guid.NewGuid(),
            BranchIds = new[] { Guid.Empty }
        });

        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == ErrorMessage.CreateBranchArea_BranchIds_Invalid);
    }

    private static CreateBranchAreaCommand CreateValidCreateCommand()
        => new()
        {
            NameEn = "Area Admin",
            UserName = "area-admin",
            Email = "area-admin@example.com",
            Password = "Password@123",
            BranchIds = new[] { Guid.NewGuid() }
        };
}

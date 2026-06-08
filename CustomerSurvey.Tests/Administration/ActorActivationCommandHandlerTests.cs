using System.Reflection;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Api.Controllers;
using CustomerSurvey.Application.Features.BranchAdmins.Command.DeactivateBranchAdmin;
using CustomerSurvey.Application.Features.BranchAdmins.Command.RestoreBranchAdmin;
using CustomerSurvey.Application.Features.DepartmentAdmins.Command.DeactivateDepartmentAdmin;
using CustomerSurvey.Application.Features.DepartmentAdmins.Command.RestoreDepartmentAdmin;
using CustomerSurvey.Application.Features.Departments.Command.RestoreDepartment;
using CustomerSurvey.Application.Features.Operators.Command.DeactivateOperator;
using CustomerSurvey.Application.Features.Operators.Command.RestoreOperator;
using CustomerSurvey.Application.Features.SuperAdmins.Command.CreateSuperAdmin;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;
using DomainUserType = CustomerSurvey.Domain.Enums.UserType;
using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

namespace CustomerSurvey.Tests.Administration;

public sealed class ActorActivationCommandHandlerTests
{
    [Fact]
    public async Task CreateSuperAdmin_CreatesActiveUserProfileAndRole_AndSavesOnce()
    {
        var currentUser = CreateUser(DomainUserType.SuperAdmin);
        var users = new InMemoryReadRepository<ApplicationUser>();
        var superAdmins = new InMemoryReadRepository<SuperAdmin>();
        var roles = new InMemoryReadRepository<Role>();
        var userWrite = new InMemoryWriteRepository<ApplicationUser>();
        var superAdminWrite = new InMemoryWriteRepository<SuperAdmin>();
        var userRoleWrite = new InMemoryWriteRepository<UserRole>();
        var unitOfWork = new TestUnitOfWork();

        users.Add(currentUser);
        superAdmins.Add(SuperAdmin.CreateSeeded(Guid.NewGuid(), currentUser.Id));
        roles.Add(Role.CreateSeeded(Guid.NewGuid(), "System Administrator", currentUser.Id));

        var handler = new CreateSuperAdminCommandHandler(
            new TestCurrentUser(currentUser.Id),
            superAdmins,
            superAdminWrite,
            users,
            userWrite,
            roles,
            userRoleWrite,
            new TestPasswordService(),
            unitOfWork);

        var result = await handler.Handle(new CreateSuperAdminCommand
        {
            NameEn = "System Admin",
            NameAr = "Admin AR",
            UserName = "admin2",
            Email = "Admin2@Company.com",
            PhoneNumber = "01000000000",
            Password = "Password123"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, userWrite.AddCount);
        Assert.Equal(1, superAdminWrite.AddCount);
        Assert.Equal(1, userRoleWrite.AddCount);
        Assert.Equal(1, unitOfWork.SaveChangesCount);

        var createdUser = Assert.Single(userWrite.AddedEntities);
        Assert.True(createdUser.IsActive);
        Assert.Equal(DomainUserType.SuperAdmin, createdUser.UserType);
        Assert.Equal("admin2", createdUser.UserName);
        Assert.Equal("admin2@company.com", createdUser.Email);

        var createdSuperAdmin = Assert.Single(superAdminWrite.AddedEntities);
        Assert.Equal(createdUser.Id, createdSuperAdmin.ApplicationUserId);
        Assert.Equal(createdUser.Id, Assert.Single(userRoleWrite.AddedEntities).ApplicationUserId);
    }

    [Fact]
    public async Task CreateSuperAdmin_Fails_WhenUnauthenticated()
    {
        var fixture = CreateSuperAdminFixture(addCurrentSuperAdmin: false, isAuthenticated: false);

        var result = await fixture.Handler.Handle(CreateSuperAdminCommand(), CancellationToken.None);

        AssertError(result, "SuperAdmins.Create.Unauthenticated");
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CreateSuperAdmin_Fails_WhenCurrentUserIsNotSuperAdmin()
    {
        var currentUser = CreateUser(DomainUserType.Operator);
        var fixture = CreateSuperAdminFixture(currentUser, addCurrentSuperAdmin: false);

        var result = await fixture.Handler.Handle(CreateSuperAdminCommand(), CancellationToken.None);

        AssertError(result, "SuperAdmins.Create.CurrentSuperAdminNotFound");
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CreateSuperAdmin_Fails_WhenUserNameAlreadyExists()
    {
        var fixture = CreateSuperAdminFixture();
        fixture.Users.Add(CreateUser(DomainUserType.Operator, userName: "admin2", email: "other@example.com"));

        var result = await fixture.Handler.Handle(CreateSuperAdminCommand(), CancellationToken.None);

        AssertError(result, "SuperAdmins.Create.DuplicateUserName");
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CreateSuperAdmin_Fails_WhenEmailAlreadyExists()
    {
        var fixture = CreateSuperAdminFixture();
        fixture.Users.Add(CreateUser(DomainUserType.Operator, userName: "other", email: "admin2@company.com"));

        var result = await fixture.Handler.Handle(CreateSuperAdminCommand(), CancellationToken.None);

        AssertError(result, "SuperAdmins.Create.DuplicateEmail");
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CreateSuperAdmin_Fails_WhenPasswordIsWeak()
    {
        var fixture = CreateSuperAdminFixture();

        var result = await fixture.Handler.Handle(CreateSuperAdminCommand(password: "weak"), CancellationToken.None);

        AssertError(result, "SuperAdmins.Create.InvalidPassword");
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task RestoreDepartment_ActivatesInactiveDepartment_AndSavesOnce()
    {
        var currentUser = CreateUser(DomainUserType.SuperAdmin);
        var users = new InMemoryReadRepository<ApplicationUser>();
        var superAdmins = new InMemoryReadRepository<SuperAdmin>();
        var departments = new InMemoryReadRepository<Department>();
        var departmentWrite = new InMemoryWriteRepository<Department>();
        var unitOfWork = new TestUnitOfWork();
        var department = Department.Create("Support", null, currentUser.Id);
        department.Deactivate();

        users.Add(currentUser);
        superAdmins.Add(SuperAdmin.CreateSeeded(Guid.NewGuid(), currentUser.Id));
        departments.Add(department);

        var handler = new RestoreDepartmentCommandHandler(
            new TestCurrentUser(currentUser.Id),
            users,
            superAdmins,
            departments,
            departmentWrite,
            unitOfWork);

        var result = await handler.Handle(new RestoreDepartmentCommand
        {
            DepartmentId = department.Id
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(department.IsActive);
        Assert.Equal(1, departmentWrite.UpdateCount);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task RestoreDepartment_Fails_WhenDepartmentNotFound()
    {
        var fixture = CreateRestoreDepartmentFixture();

        var result = await fixture.Handler.Handle(new RestoreDepartmentCommand
        {
            DepartmentId = Guid.NewGuid()
        }, CancellationToken.None);

        AssertError(result, "Departments.Restore.DepartmentNotFound");
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task RestoreDepartment_Fails_WhenCurrentUserIsNotSuperAdmin()
    {
        var fixture = CreateRestoreDepartmentFixture(addCurrentSuperAdmin: false);

        var result = await fixture.Handler.Handle(new RestoreDepartmentCommand
        {
            DepartmentId = Guid.NewGuid()
        }, CancellationToken.None);

        AssertError(result, "Departments.Restore.CurrentSuperAdminNotFound");
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task SuperAdmin_CanDeactivateAndRestoreBranchAdmin()
    {
        var fixture = CreateBranchAdminFixture();

        var deactivate = await fixture.DeactivateHandler.Handle(new DeactivateBranchAdminCommand
        {
            BranchAdminId = fixture.BranchAdmin.Id
        }, CancellationToken.None);

        Assert.True(deactivate.IsSuccess);
        Assert.False(fixture.TargetUser.IsActive);
        Assert.Equal(1, fixture.UserWrite.UpdateCount);
        Assert.Equal(1, fixture.UnitOfWork.SaveChangesCount);

        var restore = await fixture.RestoreHandler.Handle(new RestoreBranchAdminCommand
        {
            BranchAdminId = fixture.BranchAdmin.Id
        }, CancellationToken.None);

        Assert.True(restore.IsSuccess);
        Assert.True(fixture.TargetUser.IsActive);
        Assert.Equal(2, fixture.UserWrite.UpdateCount);
        Assert.Equal(2, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task NonSuperAdmin_CannotDeactivateBranchAdmin()
    {
        var fixture = CreateBranchAdminFixture(addCurrentSuperAdmin: false);

        var result = await fixture.DeactivateHandler.Handle(new DeactivateBranchAdminCommand
        {
            BranchAdminId = fixture.BranchAdmin.Id
        }, CancellationToken.None);

        AssertError(result, "BranchAdmins.Deactivate.CurrentSuperAdminNotFound");
        Assert.True(fixture.TargetUser.IsActive);
        Assert.Equal(0, fixture.UserWrite.UpdateCount);
    }

    [Fact]
    public async Task SuperAdmin_CanDeactivateAndRestoreDepartmentAdmin()
    {
        var fixture = CreateDepartmentAdminFixture();

        var deactivate = await fixture.DeactivateHandler.Handle(new DeactivateDepartmentAdminCommand
        {
            DepartmentAdminId = fixture.DepartmentAdmin.Id
        }, CancellationToken.None);

        Assert.True(deactivate.IsSuccess);
        Assert.False(fixture.TargetUser.IsActive);

        var restore = await fixture.RestoreHandler.Handle(new RestoreDepartmentAdminCommand
        {
            DepartmentAdminId = fixture.DepartmentAdmin.Id
        }, CancellationToken.None);

        Assert.True(restore.IsSuccess);
        Assert.True(fixture.TargetUser.IsActive);
        Assert.Equal(2, fixture.UserWrite.UpdateCount);
        Assert.Equal(2, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task NonSuperAdmin_CannotRestoreDepartmentAdmin()
    {
        var fixture = CreateDepartmentAdminFixture(addCurrentSuperAdmin: false);
        fixture.TargetUser.Deactivate();

        var result = await fixture.RestoreHandler.Handle(new RestoreDepartmentAdminCommand
        {
            DepartmentAdminId = fixture.DepartmentAdmin.Id
        }, CancellationToken.None);

        AssertError(result, "DepartmentAdmins.Restore.CurrentSuperAdminNotFound");
        Assert.False(fixture.TargetUser.IsActive);
        Assert.Equal(0, fixture.UserWrite.UpdateCount);
    }

    [Fact]
    public async Task DepartmentAdmin_CanDeactivateAndRestoreOperatorInOwnDepartment()
    {
        var fixture = CreateOperatorFixture();

        var deactivate = await fixture.DeactivateHandler.Handle(new DeactivateOperatorCommand
        {
            OperatorId = fixture.Operator.Id
        }, CancellationToken.None);

        Assert.True(deactivate.IsSuccess);
        Assert.False(fixture.TargetUser.IsActive);

        var restore = await fixture.RestoreHandler.Handle(new RestoreOperatorCommand
        {
            OperatorId = fixture.Operator.Id
        }, CancellationToken.None);

        Assert.True(restore.IsSuccess);
        Assert.True(fixture.TargetUser.IsActive);
        Assert.Equal(2, fixture.UserWrite.UpdateCount);
        Assert.Equal(2, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task DepartmentAdmin_CannotDeactivateOperatorInAnotherDepartment()
    {
        var fixture = CreateOperatorFixture(targetDepartmentId: Guid.NewGuid());

        var result = await fixture.DeactivateHandler.Handle(new DeactivateOperatorCommand
        {
            OperatorId = fixture.Operator.Id
        }, CancellationToken.None);

        AssertError(result, "Operators.Deactivate.Forbidden");
        Assert.True(fixture.TargetUser.IsActive);
        Assert.Equal(0, fixture.UserWrite.UpdateCount);
    }

    [Fact]
    public async Task DepartmentAdmin_CannotRestoreOperatorInAnotherDepartment()
    {
        var fixture = CreateOperatorFixture(targetDepartmentId: Guid.NewGuid());
        fixture.TargetUser.Deactivate();

        var result = await fixture.RestoreHandler.Handle(new RestoreOperatorCommand
        {
            OperatorId = fixture.Operator.Id
        }, CancellationToken.None);

        AssertError(result, "Operators.Restore.Forbidden");
        Assert.False(fixture.TargetUser.IsActive);
        Assert.Equal(0, fixture.UserWrite.UpdateCount);
    }

    [Fact]
    public void SuperAdmin_HasNoDeactivateRestoreOrDeleteSurface()
    {
        var applicationTypes = typeof(CreateSuperAdminCommand).Assembly.GetTypes();

        Assert.DoesNotContain(applicationTypes, type =>
            type.Name.Contains("DeactivateSuperAdmin", StringComparison.OrdinalIgnoreCase) ||
            type.Name.Contains("RestoreSuperAdmin", StringComparison.OrdinalIgnoreCase) ||
            type.Name.Contains("DeleteSuperAdmin", StringComparison.OrdinalIgnoreCase));

        var superAdminActions = typeof(SuperAdminsController).GetMethods(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.DoesNotContain(superAdminActions, action =>
            action.Name.Contains("Deactivate", StringComparison.OrdinalIgnoreCase) ||
            action.Name.Contains("Restore", StringComparison.OrdinalIgnoreCase) ||
            action.Name.Contains("Delete", StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(superAdminActions, action =>
            action.GetCustomAttributes<HttpDeleteAttribute>().Any() ||
            action.GetCustomAttributes<HttpPutAttribute>().Any());
    }

    private static CreateSuperAdminCommand CreateSuperAdminCommand(string password = "Password123")
        => new()
        {
            NameEn = "System Admin",
            UserName = "admin2",
            Email = "admin2@company.com",
            PhoneNumber = "01000000000",
            Password = password
        };

    private static CreateSuperAdminTestFixture CreateSuperAdminFixture(
        bool addCurrentSuperAdmin = true,
        bool isAuthenticated = true)
        => CreateSuperAdminFixture(CreateUser(DomainUserType.SuperAdmin), addCurrentSuperAdmin, isAuthenticated);

    private static CreateSuperAdminTestFixture CreateSuperAdminFixture(
        ApplicationUser currentUser,
        bool addCurrentSuperAdmin,
        bool isAuthenticated = true)
    {
        var users = new InMemoryReadRepository<ApplicationUser>();
        var superAdmins = new InMemoryReadRepository<SuperAdmin>();
        var roles = new InMemoryReadRepository<Role>();
        var userWrite = new InMemoryWriteRepository<ApplicationUser>();
        var superAdminWrite = new InMemoryWriteRepository<SuperAdmin>();
        var userRoleWrite = new InMemoryWriteRepository<UserRole>();
        var unitOfWork = new TestUnitOfWork();

        users.Add(currentUser);
        roles.Add(Role.CreateSeeded(Guid.NewGuid(), "System Administrator", currentUser.Id));

        if (addCurrentSuperAdmin)
        {
            superAdmins.Add(SuperAdmin.CreateSeeded(Guid.NewGuid(), currentUser.Id));
        }

        var handler = new CreateSuperAdminCommandHandler(
            new TestCurrentUser(isAuthenticated ? currentUser.Id : null, isAuthenticated),
            superAdmins,
            superAdminWrite,
            users,
            userWrite,
            roles,
            userRoleWrite,
            new TestPasswordService(),
            unitOfWork);

        return new CreateSuperAdminTestFixture(handler, users, unitOfWork);
    }

    private static RestoreDepartmentFixture CreateRestoreDepartmentFixture(bool addCurrentSuperAdmin = true)
    {
        var currentUser = CreateUser(DomainUserType.SuperAdmin);
        var users = new InMemoryReadRepository<ApplicationUser>();
        var superAdmins = new InMemoryReadRepository<SuperAdmin>();
        var departments = new InMemoryReadRepository<Department>();
        var departmentWrite = new InMemoryWriteRepository<Department>();
        var unitOfWork = new TestUnitOfWork();

        users.Add(currentUser);
        if (addCurrentSuperAdmin)
        {
            superAdmins.Add(SuperAdmin.CreateSeeded(Guid.NewGuid(), currentUser.Id));
        }

        var handler = new RestoreDepartmentCommandHandler(
            new TestCurrentUser(currentUser.Id),
            users,
            superAdmins,
            departments,
            departmentWrite,
            unitOfWork);

        return new RestoreDepartmentFixture(handler, unitOfWork);
    }

    private static BranchAdminFixture CreateBranchAdminFixture(bool addCurrentSuperAdmin = true)
    {
        var currentUser = CreateUser(DomainUserType.SuperAdmin);
        var targetUser = CreateUser(DomainUserType.BranchAdmin);
        var users = new InMemoryReadRepository<ApplicationUser>();
        var superAdmins = new InMemoryReadRepository<SuperAdmin>();
        var branchAdmins = new InMemoryReadRepository<BranchAdmin>();
        var userWrite = new InMemoryWriteRepository<ApplicationUser>();
        var unitOfWork = new TestUnitOfWork();
        var branchAdmin = BranchAdmin.Create(targetUser.Id, Guid.NewGuid(), currentUser.Id);

        users.AddRange([currentUser, targetUser]);
        branchAdmins.Add(branchAdmin);
        if (addCurrentSuperAdmin)
        {
            superAdmins.Add(SuperAdmin.CreateSeeded(Guid.NewGuid(), currentUser.Id));
        }

        var deactivate = new DeactivateBranchAdminCommandHandler(
            new TestCurrentUser(currentUser.Id),
            users,
            userWrite,
            superAdmins,
            branchAdmins,
            unitOfWork);

        var restore = new RestoreBranchAdminCommandHandler(
            new TestCurrentUser(currentUser.Id),
            users,
            userWrite,
            superAdmins,
            branchAdmins,
            unitOfWork);

        return new BranchAdminFixture(deactivate, restore, targetUser, branchAdmin, userWrite, unitOfWork);
    }

    private static DepartmentAdminFixture CreateDepartmentAdminFixture(bool addCurrentSuperAdmin = true)
    {
        var currentUser = CreateUser(DomainUserType.SuperAdmin);
        var targetUser = CreateUser(DomainUserType.DepartmentAdmin);
        var users = new InMemoryReadRepository<ApplicationUser>();
        var superAdmins = new InMemoryReadRepository<SuperAdmin>();
        var departmentAdmins = new InMemoryReadRepository<DepartmentAdmin>();
        var userWrite = new InMemoryWriteRepository<ApplicationUser>();
        var unitOfWork = new TestUnitOfWork();
        var departmentAdmin = DepartmentAdmin.Create(targetUser.Id, Guid.NewGuid(), currentUser.Id);

        users.AddRange([currentUser, targetUser]);
        departmentAdmins.Add(departmentAdmin);
        if (addCurrentSuperAdmin)
        {
            superAdmins.Add(SuperAdmin.CreateSeeded(Guid.NewGuid(), currentUser.Id));
        }

        var deactivate = new DeactivateDepartmentAdminCommandHandler(
            new TestCurrentUser(currentUser.Id),
            users,
            userWrite,
            superAdmins,
            departmentAdmins,
            unitOfWork);

        var restore = new RestoreDepartmentAdminCommandHandler(
            new TestCurrentUser(currentUser.Id),
            users,
            userWrite,
            superAdmins,
            departmentAdmins,
            unitOfWork);

        return new DepartmentAdminFixture(deactivate, restore, targetUser, departmentAdmin, userWrite, unitOfWork);
    }

    private static OperatorFixture CreateOperatorFixture(Guid? targetDepartmentId = null)
    {
        var departmentId = Guid.NewGuid();
        var currentUser = CreateUser(DomainUserType.DepartmentAdmin);
        var targetUser = CreateUser(DomainUserType.Operator);
        var users = new InMemoryReadRepository<ApplicationUser>();
        var departmentAdmins = new InMemoryReadRepository<DepartmentAdmin>();
        var operators = new InMemoryReadRepository<DomainOperator>();
        var userWrite = new InMemoryWriteRepository<ApplicationUser>();
        var unitOfWork = new TestUnitOfWork();
        var departmentAdmin = DepartmentAdmin.Create(currentUser.Id, departmentId, currentUser.Id);
        var operatorProfile = DomainOperator.Create(
            targetUser.Id,
            targetDepartmentId ?? departmentId,
            currentUser.Id);

        users.AddRange([currentUser, targetUser]);
        departmentAdmins.Add(departmentAdmin);
        operators.Add(operatorProfile);

        var deactivate = new DeactivateOperatorCommandHandler(
            new TestCurrentUser(currentUser.Id),
            users,
            userWrite,
            departmentAdmins,
            operators,
            unitOfWork);

        var restore = new RestoreOperatorCommandHandler(
            new TestCurrentUser(currentUser.Id),
            users,
            userWrite,
            departmentAdmins,
            operators,
            unitOfWork);

        return new OperatorFixture(deactivate, restore, targetUser, operatorProfile, userWrite, unitOfWork);
    }

    private static ApplicationUser CreateUser(
        DomainUserType userType,
        string? userName = null,
        string? email = null)
        => ApplicationUser.Create(
            userName: userName ?? $"{userType}-{Guid.NewGuid():N}",
            email: email ?? $"{Guid.NewGuid():N}@example.com",
            nameEn: userType.ToString(),
            nameAr: null,
            phoneNumber: null,
            passwordHash: $"hash-{Guid.NewGuid():N}",
            userType: userType,
            createdByApplicationUserId: Guid.NewGuid());

    private static void AssertError<T>(Result<T> result, string code)
    {
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == code);
    }

    private sealed record CreateSuperAdminTestFixture(
        CreateSuperAdminCommandHandler Handler,
        InMemoryReadRepository<ApplicationUser> Users,
        TestUnitOfWork UnitOfWork);

    private sealed record RestoreDepartmentFixture(
        RestoreDepartmentCommandHandler Handler,
        TestUnitOfWork UnitOfWork);

    private sealed record BranchAdminFixture(
        DeactivateBranchAdminCommandHandler DeactivateHandler,
        RestoreBranchAdminCommandHandler RestoreHandler,
        ApplicationUser TargetUser,
        BranchAdmin BranchAdmin,
        InMemoryWriteRepository<ApplicationUser> UserWrite,
        TestUnitOfWork UnitOfWork);

    private sealed record DepartmentAdminFixture(
        DeactivateDepartmentAdminCommandHandler DeactivateHandler,
        RestoreDepartmentAdminCommandHandler RestoreHandler,
        ApplicationUser TargetUser,
        DepartmentAdmin DepartmentAdmin,
        InMemoryWriteRepository<ApplicationUser> UserWrite,
        TestUnitOfWork UnitOfWork);

    private sealed record OperatorFixture(
        DeactivateOperatorCommandHandler DeactivateHandler,
        RestoreOperatorCommandHandler RestoreHandler,
        ApplicationUser TargetUser,
        DomainOperator Operator,
        InMemoryWriteRepository<ApplicationUser> UserWrite,
        TestUnitOfWork UnitOfWork);
}

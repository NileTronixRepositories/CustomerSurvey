using System.Data;
using System.Linq.Expressions;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Auth.Command.ResetUserPassword;
using CustomerSurvey.Domain.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using DomainUserType = CustomerSurvey.Domain.Enums.UserType;
using SecurityUserType = BuildingBlock.Application.Abstraction.Security.UserType;
using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

namespace CustomerSurvey.Tests.Auth;

public sealed class ResetUserPasswordCommandHandlerTests
{
    [Fact]
    public async Task SuperAdmin_CanReset_BranchAreaPassword()
    {
        var superAdminUser = CreateUser(DomainUserType.SuperAdmin);
        var targetUser = CreateUser(DomainUserType.BranchArea);
        var oldPasswordHash = targetUser.PasswordHash;
        var fixture = CreateFixture(currentApplicationUserId: superAdminUser.Id);

        fixture.SuperAdmins.Add(SuperAdmin.CreateSeeded(Guid.NewGuid(), superAdminUser.Id));
        fixture.ApplicationUsers.Add(targetUser);
        fixture.BranchAreas.Add(BranchArea.Create(targetUser.Id, superAdminUser.Id));

        var result = await fixture.Handler.Handle(CreateCommand(targetUser.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(oldPasswordHash, targetUser.PasswordHash);
        Assert.True(targetUser.IsFirstLogin);
        Assert.Equal(1, fixture.ApplicationUserWriteRepository.UpdateCount);
        Assert.Same(targetUser, fixture.ApplicationUserWriteRepository.UpdatedEntities.Single());
        Assert.Equal(1, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task BranchAdmin_CannotReset_BranchAreaPassword()
    {
        var branchAdminUser = CreateUser(DomainUserType.BranchAdmin);
        var targetUser = CreateUser(DomainUserType.BranchArea);
        var oldPasswordHash = targetUser.PasswordHash;
        var fixture = CreateFixture(currentApplicationUserId: branchAdminUser.Id);

        fixture.BranchAdmins.Add(BranchAdmin.Create(
            branchAdminUser.Id,
            Guid.NewGuid(),
            branchAdminUser.Id));
        fixture.ApplicationUsers.Add(targetUser);
        fixture.BranchAreas.Add(BranchArea.Create(targetUser.Id, branchAdminUser.Id));

        var result = await fixture.Handler.Handle(CreateCommand(targetUser.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error =>
            error.Type == ErrorType.Security &&
            error.Code == "Users.ResetPassword.ForbiddenTargetType");
        Assert.Equal(oldPasswordHash, targetUser.PasswordHash);
        Assert.Equal(0, fixture.ApplicationUserWriteRepository.UpdateCount);
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task BranchArea_CannotReset_AnotherBranchAreaPassword()
    {
        var branchAreaActorUser = CreateUser(DomainUserType.BranchArea);
        var targetUser = CreateUser(DomainUserType.BranchArea);
        var oldPasswordHash = targetUser.PasswordHash;
        var fixture = CreateFixture(currentApplicationUserId: branchAreaActorUser.Id);

        fixture.BranchAreas.Add(BranchArea.Create(branchAreaActorUser.Id, branchAreaActorUser.Id));
        fixture.ApplicationUsers.Add(targetUser);
        fixture.BranchAreas.Add(BranchArea.Create(targetUser.Id, branchAreaActorUser.Id));

        var result = await fixture.Handler.Handle(CreateCommand(targetUser.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(oldPasswordHash, targetUser.PasswordHash);
        Assert.Equal(0, fixture.ApplicationUserWriteRepository.UpdateCount);
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task UnsupportedTargetProfile_StillFails()
    {
        var superAdminUser = CreateUser(DomainUserType.SuperAdmin);
        var targetUser = CreateUser(DomainUserType.BranchArea);
        var oldPasswordHash = targetUser.PasswordHash;
        var fixture = CreateFixture(currentApplicationUserId: superAdminUser.Id);

        fixture.SuperAdmins.Add(SuperAdmin.CreateSeeded(Guid.NewGuid(), superAdminUser.Id));
        fixture.ApplicationUsers.Add(targetUser);

        var result = await fixture.Handler.Handle(CreateCommand(targetUser.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error =>
            error.Type == ErrorType.Security &&
            error.Code == "Users.ResetPassword.TargetProfileNotSupported");
        Assert.Equal(oldPasswordHash, targetUser.PasswordHash);
        Assert.Equal(0, fixture.ApplicationUserWriteRepository.UpdateCount);
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCount);
    }

    private static ResetUserPasswordCommand CreateCommand(Guid applicationUserId)
        => new()
        {
            ApplicationUserId = applicationUserId,
            NewPassword = "NewPassword123",
            ConfirmNewPassword = "NewPassword123"
        };

    private static ApplicationUser CreateUser(DomainUserType userType)
        => ApplicationUser.Create(
            userName: $"{userType}-{Guid.NewGuid():N}",
            email: $"{Guid.NewGuid():N}@example.com",
            nameEn: userType.ToString(),
            nameAr: null,
            phoneNumber: null,
            passwordHash: $"old-password-hash-{Guid.NewGuid():N}",
            userType: userType,
            createdByApplicationUserId: Guid.NewGuid());

    private static HandlerFixture CreateFixture(Guid currentApplicationUserId)
    {
        var applicationUsers = new InMemoryReadRepository<ApplicationUser>();
        var superAdmins = new InMemoryReadRepository<SuperAdmin>();
        var branchAdmins = new InMemoryReadRepository<BranchAdmin>();
        var branchAreas = new InMemoryReadRepository<BranchArea>();
        var departmentAdmins = new InMemoryReadRepository<DepartmentAdmin>();
        var branchUsers = new InMemoryReadRepository<BranchUser>();
        var operators = new InMemoryReadRepository<DomainOperator>();
        var applicationUserWriteRepository = new InMemoryWriteRepository<ApplicationUser>();
        var currentUser = new TestCurrentUser(currentApplicationUserId);
        var unitOfWork = new TestUnitOfWork();

        var handler = new ResetUserPasswordCommandHandler(
            applicationUsers,
            applicationUserWriteRepository,
            superAdmins,
            branchAdmins,
            branchAreas,
            departmentAdmins,
            branchUsers,
            operators,
            currentUser,
            unitOfWork);

        return new HandlerFixture(
            handler,
            applicationUsers,
            superAdmins,
            branchAdmins,
            branchAreas,
            departmentAdmins,
            branchUsers,
            operators,
            applicationUserWriteRepository,
            unitOfWork);
    }

    private sealed record HandlerFixture(
        ResetUserPasswordCommandHandler Handler,
        InMemoryReadRepository<ApplicationUser> ApplicationUsers,
        InMemoryReadRepository<SuperAdmin> SuperAdmins,
        InMemoryReadRepository<BranchAdmin> BranchAdmins,
        InMemoryReadRepository<BranchArea> BranchAreas,
        InMemoryReadRepository<DepartmentAdmin> DepartmentAdmins,
        InMemoryReadRepository<BranchUser> BranchUsers,
        InMemoryReadRepository<DomainOperator> Operators,
        InMemoryWriteRepository<ApplicationUser> ApplicationUserWriteRepository,
        TestUnitOfWork UnitOfWork);

    private sealed class TestCurrentUser : ICurrentUser
    {
        public TestCurrentUser(Guid userId)
        {
            UserId = userId;
        }

        public bool IsAuthenticated => true;

        public Guid? UserId { get; }

        public Guid? AccountId => null;

        public Guid? ActiveBranchId => null;

        public string? Role => null;

        public SecurityUserType UserType => SecurityUserType.Unknown;

        public int? UserTypeValue => null;
    }

    private sealed class InMemoryReadRepository<TEntity> : IWriteReadRepository<TEntity>
        where TEntity : class
    {
        private readonly List<TEntity> _entities = new();

        public void Add(TEntity entity)
        {
            _entities.Add(entity);
        }

        public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(FindById(id));

        public Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(FindById(id));

        public Task<TEntity?> GetByIdAsync(long id, CancellationToken ct = default)
            => Task.FromResult(FindById(id));

        public Task<TEntity?> GetByIdAsync(string id, CancellationToken ct = default)
            => Task.FromResult(FindById(id));

        public Task<TEntity?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default)
            => GetByIdAsync(id, ct);

        public Task<TEntity?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
            => GetByIdAsync(id, ct);

        public Task<TEntity?> GetByIdTrackedAsync(long id, CancellationToken ct = default)
            => GetByIdAsync(id, ct);

        public Task<TEntity?> GetByIdTrackedAsync(string id, CancellationToken ct = default)
            => GetByIdAsync(id, ct);

        public Task<TEntity?> GetByPropertyAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken ct = default)
            => Task.FromResult(_entities.FirstOrDefault(predicate.Compile()));

        public Task<TEntity?> GetByPropertyTrackedAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken ct = default)
            => GetByPropertyAsync(predicate, ct);

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
            => Task.FromResult(_entities.Any(predicate.Compile()));

        public Task<int> CountAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken ct = default)
            => Task.FromResult(predicate is null
                ? _entities.Count
                : _entities.Count(predicate.Compile()));

        public Task<List<TEntity>> ListAsync(Specification<TEntity> spec, CancellationToken ct = default)
            => Task.FromResult(_entities.Where(spec.Criteria.Compile()).ToList());

        public Task<TEntity?> FirstOrDefaultAsync(Specification<TEntity> spec, CancellationToken ct = default)
            => Task.FromResult(_entities.FirstOrDefault(spec.Criteria.Compile()));

        public Task<List<TOut>> ListAsync<TOut>(
            Specification<TEntity, TOut> spec,
            CancellationToken ct = default)
        {
            var predicate = spec.Criteria.Compile();
            var selector = spec.Selector.Compile();

            return Task.FromResult(_entities.Where(predicate).Select(selector).ToList());
        }

        public Task<TOut?> FirstOrDefaultAsync<TOut>(
            Specification<TEntity, TOut> spec,
            CancellationToken ct = default)
        {
            var predicate = spec.Criteria.Compile();
            var selector = spec.Selector.Compile();
            var entity = _entities.FirstOrDefault(predicate);

            return Task.FromResult(entity is null ? default : selector(entity));
        }

        public IQueryable<TEntity> Query()
            => _entities.AsQueryable();

        public async Task<(List<TEntity> Data, int Count)> ListWithCountAsync(
            Specification<TEntity> spec,
            CancellationToken ct = default)
        {
            var data = await ListAsync(spec, ct);
            return (data, data.Count);
        }

        public async Task<(List<TOut> Data, int Count)> ListWithCountAsync<TOut>(
            Specification<TEntity, TOut> spec,
            CancellationToken ct = default)
        {
            var data = await ListAsync(spec, ct);
            return (data, data.Count);
        }

        private TEntity? FindById<TId>(TId id)
            => _entities.FirstOrDefault(entity => Equals(GetId(entity), id));

        private static object? GetId(TEntity entity)
            => typeof(TEntity).GetProperty("Id")?.GetValue(entity);
    }

    private sealed class InMemoryWriteRepository<TEntity> : IWriteRepository<TEntity>
        where TEntity : class
    {
        private readonly List<TEntity> _updatedEntities = new();

        public IReadOnlyList<TEntity> UpdatedEntities => _updatedEntities;

        public int UpdateCount => _updatedEntities.Count;

        public Task AddAsync(TEntity entity, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task AddRangeAsync(List<TEntity> entities, CancellationToken ct = default)
            => Task.CompletedTask;

        public void Update(TEntity entity)
        {
            _updatedEntities.Add(entity);
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            _updatedEntities.AddRange(entities);
        }

        public void Delete(TEntity entity)
        {
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
        {
        }
    }

    private sealed class TestUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCount { get; private set; }

        public BuildingBlock.Application.Repositories.IWriteRepository<TEntity, PlatformWriteMarker>
            WriteRepository<TEntity>()
            where TEntity : class
            => throw new NotSupportedException();

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            SaveChangesCount++;
            return Task.FromResult(1);
        }

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel,
            CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task CommitTransactionAsync(CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task RollbackTransactionAsync(CancellationToken ct = default)
            => throw new NotSupportedException();
    }
}

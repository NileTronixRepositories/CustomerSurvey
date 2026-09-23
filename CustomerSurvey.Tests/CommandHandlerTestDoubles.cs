using System.Data;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Abstraction.Presistence;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query;
using SecurityUserType = BuildingBlock.Application.Abstraction.Security.UserType;

namespace CustomerSurvey.Tests;

internal sealed class TestCurrentUser : ICurrentUser
{
    public TestCurrentUser(Guid? userId, bool isAuthenticated = true)
    {
        UserId = userId;
        IsAuthenticated = isAuthenticated;
    }

    public bool IsAuthenticated { get; }

    public Guid? UserId { get; }

    public Guid? AccountId => null;

    public Guid? ActiveBranchId => null;

    public string? Role => null;

    public SecurityUserType UserType => SecurityUserType.Unknown;

    public int? UserTypeValue => null;
}

internal sealed class TestPasswordService : IPasswordService
{
    public string Hash(string password)
        => $"hashed::{password}";

    public bool Verify(string password, string passwordHash)
        => passwordHash == Hash(password);

    public Task<string> HashAsync(string password, CancellationToken ct = default)
        => Task.FromResult(Hash(password));

    public Task<bool> VerifyAsync(string password, string passwordHash, CancellationToken ct = default)
        => Task.FromResult(Verify(password, passwordHash));

    public bool IsStrongPassword(string password)
        => password.Length >= 8 &&
           password.Any(char.IsUpper) &&
           password.Any(char.IsLower) &&
           password.Any(char.IsDigit);
}

internal sealed class InMemoryReadRepository<TEntity> : IWriteReadRepository<TEntity>
    where TEntity : class
{
    private readonly List<TEntity> _entities = new();

    public IReadOnlyList<TEntity> Entities => _entities;

    public void Add(TEntity entity)
        => _entities.Add(entity);

    public void AddRange(IEnumerable<TEntity> entities)
        => _entities.AddRange(entities);

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
        => new TestAsyncEnumerable<TEntity>(_entities);

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

internal sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
        => new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression)
        => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression)
        => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(
        Expression expression,
        CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethods()
            .Single(method =>
                method.Name == nameof(IQueryProvider.Execute) &&
                method.IsGenericMethod &&
                method.GetParameters().Length == 1)
            .MakeGenericMethod(resultType)
            .Invoke(_inner, new object[] { expression });

        return (TResult)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal sealed class TestAsyncEnumerable<T>
    : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(
        CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(((IEnumerable<T>)this).GetEnumerator());

    IQueryProvider IQueryable.Provider
        => new TestAsyncQueryProvider<T>(this);
}

internal sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
        => ValueTask.FromResult(_inner.MoveNext());

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}

internal sealed class InMemoryWriteRepository<TEntity> : IWriteRepository<TEntity>
    where TEntity : class
{
    private readonly List<TEntity> _addedEntities = new();
    private readonly List<TEntity> _updatedEntities = new();

    public IReadOnlyList<TEntity> AddedEntities => _addedEntities;

    public IReadOnlyList<TEntity> UpdatedEntities => _updatedEntities;

    public int AddCount => _addedEntities.Count;

    public int UpdateCount => _updatedEntities.Count;

    public Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        _addedEntities.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(List<TEntity> entities, CancellationToken ct = default)
    {
        _addedEntities.AddRange(entities);
        return Task.CompletedTask;
    }

    public void Update(TEntity entity)
        => _updatedEntities.Add(entity);

    public void UpdateRange(IEnumerable<TEntity> entities)
        => _updatedEntities.AddRange(entities);

    public void Delete(TEntity entity)
    {
    }

    public void DeleteRange(IEnumerable<TEntity> entities)
    {
    }
}

internal sealed class TestUnitOfWork : IUnitOfWork
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

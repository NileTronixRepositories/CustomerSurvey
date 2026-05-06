using System.Collections.Concurrent;

namespace BuildingBlock.Application.Abstraction.Caching
{
    public sealed class KeyedSemaphore
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public async Task<IDisposable> WaitAsync(string key, CancellationToken ct)
        {
            var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await sem.WaitAsync(ct);
            return new Releaser(key, sem, _locks);
        }

        private sealed class Releaser : IDisposable
        {
            private readonly string _key;
            private readonly SemaphoreSlim _sem;
            private readonly ConcurrentDictionary<string, SemaphoreSlim> _dict;

            public Releaser(string key, SemaphoreSlim sem, ConcurrentDictionary<string, SemaphoreSlim> dict)
                => (_key, _sem, _dict) = (key, sem, dict);

            public void Dispose()
            {
                _sem.Release();
                if (_sem.CurrentCount == 1)
                    _dict.TryRemove(_key, out _);
            }
        }
    }
}
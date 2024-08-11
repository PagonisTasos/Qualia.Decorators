using Qualia.Decorators.Framework;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Qualia.Decorators
{
    public class Lock : DecoratorBehaviorAsync
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
        private readonly ConcurrentDictionary<string, int> _lockReferences = new ConcurrentDictionary<string, int>();

        public override async Task<TReturn> InvokeAsync<TDecorated, TReturn>(DecoratorContext<TDecorated> context)
        {
            var lockingKey = $"{nameof(TDecorated)}_{context.TargetMethod.Name}";

            try
            {
                await LockAsync(lockingKey);

                return await Next<TDecorated, TReturn>(context);
            }
            finally
            {
                await UnlockAsync(lockingKey);
            }
        }

        private async Task LockAsync(string lockingKey)
        {
            var @lock = await AcquireLockAndReference(lockingKey);
            await @lock.WaitAsync();
        }

        private Task UnlockAsync(string lockingKey)
        {
            return ReleaseLockAndReference(lockingKey);
        }

        private async Task<SemaphoreSlim> AcquireLockAndReference(string lockingKey)
        {
            try
            {
                await _semaphore.WaitAsync();

                if (_locks.TryGetValue(lockingKey, out SemaphoreSlim @lock))
                {
                    _lockReferences[lockingKey]++;
                    return @lock;
                }

                @lock = new SemaphoreSlim(1);
                _locks.AddOrUpdate(lockingKey, x => @lock, (y, z) => @lock);
                _lockReferences.AddOrUpdate(lockingKey, x => 1, (y, z) => 1);

                return @lock;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task ReleaseLockAndReference(string lockingKey)
        {
            try
            {
                await _semaphore.WaitAsync();

                if (_locks.TryGetValue(lockingKey, out SemaphoreSlim @lock))
                {
                    _lockReferences[lockingKey]--;
                    @lock.Release();

                    if (_lockReferences[lockingKey] <= 0)
                    {
                        _locks.TryRemove(lockingKey, out _);
                        _lockReferences.TryRemove(lockingKey, out _);
                        @lock.Dispose();
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
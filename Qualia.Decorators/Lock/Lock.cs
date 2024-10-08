﻿using Qualia.Decorators.Framework;
using System.Collections.Concurrent;

namespace Qualia.Decorators
{
    public class Lock : DecoratorBehaviorAsync
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
        private readonly ConcurrentDictionary<string, int> _lockReferences = new();

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

                if (_locks.TryGetValue(lockingKey, out SemaphoreSlim? @lock))
                {
                    _lockReferences[lockingKey]++;
                    return @lock;
                }

                @lock = new SemaphoreSlim(1);
                _locks.AddOrUpdate(lockingKey, _ => @lock, (_, _) => @lock);
                _lockReferences.AddOrUpdate(lockingKey, _ => 1, (_, _) => 1);

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

                if (_locks.TryGetValue(lockingKey, out SemaphoreSlim? @lock))
                {
                    _lockReferences[lockingKey]--;
                    @lock.Release();

                    if (_lockReferences[lockingKey] <= 0)
                    {
                        _locks.Remove(lockingKey, out _);
                        _lockReferences.Remove(lockingKey, out _);
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
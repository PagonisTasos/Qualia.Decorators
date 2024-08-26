
# Qualia.Decorators Library

The Decorators library is a powerful tool for C# developers that allows you to intercept methods and execute code before and after.

The library uses attributes to mark the targeted methods and - behind the scenes - adds the desired behavior via decorator pattern, providing additional functionality without modifying the initial implementation.

## Features

- Memoize: Cache a method result for the lifetime of the object containing it.
- MemCache: Cache a method result.
- Lock: Lock a method, to make it thread safe.
- Open to extensions: Add your own attributes, using custom logic.


## Usage


```csharp
//ex: Use Memoize on a method
public class Foo
{
    [Memoize]
    public void Bar()
    {
        // Your method implementation
    }
}
```

```csharp
// To enable the attributes you need to add the targeted class as a service to the service collection
// and then call services.UseDecorators method. The targeted class must be added behind an interface.
public void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<IFoo, Foo>();
    services.UseDecorators();
}
```

Please note that the Decorators library uses reflection and should be used judiciously considering its impact on performance.


## Example of custom decorator behavior


To extend the library, simply implement DecoratorBehavior (or DecoratorBehaviorAsync), and create a corresponding attribute.

```csharp
    public class Memoize : DecoratorBehavior
    {
        private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        public override object Invoke<TDecorated>(DecoratorContext<TDecorated> context)
        {
            var key = KeyGenerator.CreateKey(context.TargetMethod, context.Args);
            var result = _cache.GetOrAdd(key, _ => Next(context));

            return result;
        }
    }

    public class MemoizeAttribute : DecorateAttribute
    {
        public MemoizeAttribute(string name = null) : base(typeof(Memoize), name) { }
    }

```


You can also inject services in your behaviors or use parameters for the attributes.

```csharp
    public class MemCache : DecoratorBehavior
    {
        private ILogger<MemCache> _logger;
        private readonly IMemoryCache _cache;

        public MemCache(ILogger<MemCache> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public override object Invoke<TDecorated>(DecoratorContext<TDecorated> context)
        {
            var key = KeyGenerator.CreateKey(context.TargetMethod, context.Args);
            var result = _cache.GetOrCreate(key, entry => 
            {
                ConfigureExpiration(ref entry, context);

                return Next(context); 
            });

            return result;
        }

        private void ConfigureExpiration<TDecorated>(ref ICacheEntry entry, DecoratorContext<TDecorated> context)
        {
            var att = (context.AssociatedDecorateAttribute as MemCacheAttribute);

            if (att?.Expiration == MemCacheAttribute.ExpirationType.Absolute)
            {
                entry.AbsoluteExpirationRelativeToNow = att?.TimeSpan;
                return;
            }
            if (att?.Expiration == MemCacheAttribute.ExpirationType.Sliding)
            {
                entry.SlidingExpiration = att?.TimeSpan;
                return;
            }

            throw new InvalidOperationException("MemCache decorator behavior failed while determining attribute's expiration type.");
        }
    }

    public class MemCacheAttribute : DecorateAttribute
    {
        public TimeSpan? TimeSpan { get; set; }
        public ExpirationType Expiration { get; set; }

        public MemCacheAttribute(string name = null, string timespan = "1m", ExpirationType expiration = ExpirationType.Absolute) 
            : base(typeof(MemCache), name) 
        {
            TimeSpan = StringToTimeSpan.Parse(timespan);
            Expiration = expiration;
        }

        public enum ExpirationType { Absolute, Sliding }
    }
```


Additionally, async behaviors can be implemented.

```csharp
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
    }

    public class LockAttribute : DecorateAttribute
    {
        public LockAttribute(string name = null) : base(typeof(Lock), name)
        { }
    }
```
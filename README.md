
# Qualia.Decorators Library

The Decorators library is a powerful tool for C# developers that allows you to add additional behavior to your classes and methods using attributes. This library uses the decorator pattern to wrap your classes and methods with additional functionality without modifying their implementation.

The Decorate attribute can be used to intercept a method call, and add behavior before and/or after the actual call. This is done without modifying the original method code.

## Features

- DecorateAttribute: This attribute can be applied to classes or methods. It takes a Type parameter that represents the decorator behavior to be applied.
- IDecoratorBehavior: This interface defines the decorator behavior. You can implement this interface to create your own custom decorators.
- DecorateIgnoreAttribute: This attribute can be applied to methods. It allows you to ignore certain class decorators for specific methods.
- ServiceCollectionExtensions: This class provides extension methods for IServiceCollection to easily add decorators to your services in an ASP.NET Core application.


## Usage

Here is an example of how to use the Decorators library:

```csharp
// Define your decorator behavior
public class MyDecoratorBehavior : IDecoratorBehavior
{
    public object? Invoke<TDecorated>(TDecorated decorated, MethodInfo targetMethod, object?[]? args)
    {
        // Add your decorator behavior here
        		
        // do things before the method call
		
        //delegate the call
        var result = targetMethod.Invoke(decorated, args);
		
        //do things after the method call
    }
}
```

```csharp
// Apply the decorator to a class or method 
[Decorate(typeof(MyDecoratorBehavior))]
public class MyClass
{
    //MyDecoratorBehavior wraps this method
    public void MyMethod()
    {
        // Your method implementation
    }

    [Decorate(typeof(FooDecoratorBehavior))]
    //both MyDecoratorBehavior and FooDecoratorBehavior wrap this method 
    public void MyOtherMethod()
    {
        // Your method implementation
    }
}
```

```csharp
// Decorators are nested in the order they are declared.
[Decorate(typeof(FooBehavior))]
[Decorate(typeof(FooBehavior))]
[Decorate(typeof(BarBehavior))]
public class MyClass
{
    //client -> foo -> foo -> bar -> MyMethod
    public void MyMethod()
    {
        // Your method implementation
    }

    [Decorate(typeof(LogBehavior))]
    [Decorate(typeof(CacheBehavior))]
    //client -> foo -> foo -> bar -> log -> cache -> MyOtherMethod
    public void MyOtherMethod()
    {
        // Your method implementation
    }
}
```

```csharp
//you can ignore class decorators
[Decorate(typeof(FooBehavior))]
[Decorate(typeof(BarBehavior))]
[Decorate(typeof(BarBehavior), "barName")]
public class MyClass
{
    //all class decorators apply
    public void MyMethod()
    {
        // Your method implementation
    }

    [DecorateIgnore] //ignore all class decorators (foo, bar, bar)
    [Decorate(typeof(LogBehavior))]
    //only log applies
    public void MyOtherMethod()
    {
        // Your method implementation
    }

    [DecorateIgnore(typeof(BarBehavior))] //ignore all class bar decorators
    [Decorate(typeof(LogBehavior))]
    //foo and log apply
    public void MyThirdMethod()
    {
        // Your method implementation
    }

    [DecorateIgnore("barName")] //ignore named decorator "barName"
    //foo and bar apply
    public void MyLastMethod()
    {
        // Your method implementation
    }
}
```

```csharp
// To enable the attributes you need to add MyClass as a service to the service collection
// and then call services.UseDecorators method. MyClass must be added behind an interface.
public void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<IMyClass, MyClass>();
    services.UseDecorators();
}
```

Please note that the Decorators library uses reflection and should be used judiciously considering its impact on performance.


## Example of custom decorator behavior

To extend the library, simply implement IDecoratorBehavior, and use the new class with the decorate attribute:

```csharp
public class Memoize : IDecoratorBehavior
{
    private ILogger<Memoize> _logger;
    private readonly ConcurrentDictionary<string, object?> _cache = new();

    //you can inject services to your constructor
    public Memoize(ILogger<Memoize> logger)
    {
        _logger = logger;
    }

    public object? Invoke<TDecorated>(TDecorated decorated, MethodInfo targetMethod, object?[]? args)
    {
        try
        {
            //get a hash of the target methods arguments to use as a cache key
            var key = GenerateCacheKey(targetMethod, args);

            //call the target method if key not in cache, and cache the key & result in memory
            //or just return the result if the key is in the cache
            var result = _cache.GetOrAdd(key, _ => targetMethod.Invoke(decorated, args));

            return result;
        }
        catch (TargetInvocationException ex)
        {
            _logger?.LogError(ex.InnerException ?? ex,
                "Error during invocation of {decoratedClass}.{methodName}", typeof(TDecorated), targetMethod?.Name);
            throw ex.InnerException ?? ex;
        }
    }

    private static string GenerateCacheKey(MethodInfo targetMethod, object?[]? args)
    {
        var serializedArgs = JsonSerializer.Serialize(args);
        byte[] bytes = Encoding.UTF8.GetBytes(serializedArgs);
        byte[] hashBytes = SHA1.HashData(bytes);
        return $"{targetMethod.Name}_{BitConverter.ToString(hashBytes).Replace("-", "")}";
    }
}
```
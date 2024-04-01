
# Qualia.Decorators Library

The Decorators library is a powerful tool for C# developers that allows you to add additional behavior to your classes and methods using attributes. This library uses the decorator pattern to wrap your classes and methods with additional functionality without modifying their implementation.


## Features

- DecorateAttribute: This attribute can be applied to classes or methods. It takes a Type parameter that represents the decorator behavior to be applied.
- DecorateIgnoreAttribute: This attribute can be applied to methods. It allows you to ignore certain decorators for specific methods.
- Decorator<TDecorated>: This class uses the DispatchProxy to create a proxy of the decorated class and apply the decorator behavior.
- IDecoratorBehavior: This interface defines the decorator behavior. You can implement this interface to create your own custom decorators.
- Memoize: This is an example of a decorator behavior that caches the result of a method call to improve performance for repeated calls with the same parameters.
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
		
		var result = targetMethod.Invoke(decorated, args);
		
		//do things after the method call
    }
}

// Apply the decorator to a class or method 
// behavior is applied in the same order as decorators are assigned to the class
[Decorate(typeof(MyDecoratorBehavior))]
[Decorate(typeof(FooDecoratorBehavior))]
[Decorate(typeof(BarDecoratorBehavior))]
[Decorate(typeof(FooDecoratorBehavior), "myDecoratorName")]
public class MyClass
{
    //all class decorators apply
    public void MyMethod()
    {
        // Your method implementation
    }

    [Decorate(typeof(MyMethodSpecificDecoratorBehavior))] //apply all class decorators plus this
    public void MySecondMethod()
    {
        // Your method implementation
    }

    [DecorateIgnore(typeof(FooDecoratorBehavior))] //ignore all class decorators of type FooDecoratorBehavior
    [Decorate(typeof(MyMethodSpecificDecoratorBehavior))] //apply all (not ignored) class decorators plus this
    public void MyThirdMethod()
    {
        // Your method implementation
    }

    [DecorateIgnore(typeof(FooDecoratorBehavior), "myDecoratorName")] //ignore only the class decorator named "myDecoratorName"
    public void MyOtherMethod()
    {
        // Your method implementation
    }

    [DecorateIgnore] //ignore all class decorators
    public void MyLastMethod()
    {
        // Your method implementation
    }
}

// In your Startup.cs file
public void ConfigureServices(IServiceCollection services)
{
    services.UseDecorators();
}
```

In this example, MyDecoratorBehavior will be applied to all methods in MyClass. You can also apply the decorator to specific methods or ignore certain decorators using the DecorateAttribute and DecorateIgnoreAttribute respectively.

Please note that the Decorators library uses reflection and should be used judiciously considering its impact on performance.


## Example of custom decorator behavior

Here is an example of how to extend the Decorators library:

```csharp
public class Memoize : IDecoratorBehavior
{
    private ILogger<Memoize> _logger;
    private readonly ConcurrentDictionary<string, object?> _cache = new();

    //inject services to your constructor
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
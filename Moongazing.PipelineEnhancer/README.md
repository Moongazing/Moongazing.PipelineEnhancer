# Moongazing Pipeline Enhancer

**Moongazing Pipeline Enhancer** is a robust and extensible library tailored for MediatR-based .NET applications. It provides a suite of prebuilt pipeline behaviors that enhance the development experience by simplifying cross-cutting concerns such as caching, logging, validation, performance tracking, authorization, and transaction management.

This library aims to streamline application development by enforcing clean architecture principles, improving code readability, and fostering reusability. Whether you're building a monolithic application or a distributed microservices system, Moongazing Pipeline Enhancer adapts seamlessly to your requirements.

---

## ✨ Features

### 🛡️ **Authorization**
- Enforce role-based access control on your requests using `AuthorizationBehavior`.

### 📦 **Caching**
- Cache request results with `CachingBehavior` to improve performance.
- Invalidate cached data on updates with `CacheRemovingBehavior`.

### 🖋️ **Logging**
- Automatically log request and response information using `LoggingBehavior` for improved debugging and monitoring.

### 🚀 **Performance Tracking**
- Track and monitor the execution time of your request handlers with `PerformanceBehavior`.

### ⚙️ **Transaction Management**
- Handle atomic operations with `TransactionScopeBehavior` to ensure consistency across multiple operations.

### ✅ **Validation**
- Validate requests using FluentValidation integrated via `ValidationBehavior`.

### 🔧 **Custom Behavior Support**
- Extend the pipeline by creating your custom behaviors with minimal effort.

---

## 🛠️ Installation

### 1. Install the NuGet Package
Add the library to your project using NuGet:


dotnet add package Moongazing.PipelineEnhancer



- Add Required Configuration

If you're using caching features, ensure you define CacheSettings in your appsettings.json file:

{
  "CacheSettings": {
    "SlidingExpiration": 30
  }
}

Register the Pipeline Behaviors

Add the pipeline behaviors to your application by registering them in the Program.cs or Startup.cs file:

var builder = WebApplication.CreateBuilder(args);

// Add MediatR and Moongazing Pipeline Enhancer behaviors
builder.Services.AddPipelineBehaviors();

var app = builder.Build();
app.Run();


🚀 Usage
1. AuthorizationBehavior

Protect your requests by implementing the ISecuredRequest interface. Specify the roles that can access the request.

public class MySecuredRequest : IRequest<MyResponse>, ISecuredRequest
{
    public string[] Roles => new[] { "Admin", "Editor" };
}

2. CachingBehavior

Leverage distributed caching by implementing ICachableRequest. Define a unique cache key, sliding expiration, and optional grouping for cache invalidation.

public class MyCachableRequest : IRequest<MyResponse>, ICachableRequest
{
    public string CacheKey => "my-unique-cache-key";
    public bool BypassCache => false;
    public string? CacheGroupKey => "my-cache-group";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(15);
}

3. CacheRemovingBehavior

Automatically remove or invalidate cached data after updates by implementing ICacheRemoverRequest.

public class MyCacheInvalidationRequest : IRequest<MyResponse>, ICacheRemoverRequest
{
    public string CacheKey => "my-unique-cache-key";
    public string? CacheGroupKey => "my-cache-group";
    public bool BypassCache => false;
}

4. LoggingBehavior

To enable logging for a request, simply implement the ILoggableRequest interface. No additional configuration is required.

public class MyLoggableRequest : IRequest<MyResponse>, ILoggableRequest
{
    public string Data { get; set; }
}

5. PerformanceBehavior

Monitor performance by specifying a threshold for request execution time using IIntervalRequest.

public class MyPerformanceRequest : IRequest<MyResponse>, IIntervalRequest
{
    public int Interval => 5; // Log warning if execution exceeds 5 seconds
}

6. TransactionScopeBehavior

Ensure atomicity across operations by implementing the ITransactionalRequest interface.

public class MyTransactionalRequest : IRequest<MyResponse>, ITransactionalRequest
{
    public int EntityId { get; set; }
}

7. ValidationBehavior

Define rules for your requests using FluentValidation. These rules will be applied automatically before the request handler is executed.

public class MyRequestValidator : AbstractValidator<MyRequest>
{
    public MyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Age).GreaterThan(0).WithMessage("Age must be greater than 0.");
    }
}

🧪 Testing

Moongazing Pipeline Enhancer is built with testability in mind. You can mock its behaviors and dependencies for unit testing.
Example: Mocking CachingBehavior

var cacheMock = new Mock<IDistributedCache>();
cacheMock.Setup(x => x.GetAsync("my-cache-key", It.IsAny<CancellationToken>()))
         .ReturnsAsync(Encoding.UTF8.GetBytes("{\"Message\":\"Cached Response\"}"));

var loggerMock = new Mock<ILogger<CachingBehavior<MyRequest, MyResponse>>>();

var cachingBehavior = new CachingBehavior<MyRequest, MyResponse>(
    cacheMock.Object,
    loggerMock.Object,
    Mock.Of<IConfiguration>()
);

Example: Mocking LoggingBehavior

var loggerMock = new Mock<ILogger<LoggingBehavior<MyRequest, MyResponse>>>();
var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

var loggingBehavior = new LoggingBehavior<MyRequest, MyResponse>(
    httpContextAccessorMock.Object,
    loggerMock.Object
);

📚 Advanced Configuration
Extending Moongazing Pipeline Enhancer

Add your own custom pipeline behaviors by implementing the IPipelineBehavior<TRequest, TResponse> interface:

public class CustomBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Pre-processing logic
        var response = await next();
        // Post-processing logic
        return response;
    }
}


🔧 Support

If you encounter any issues or have questions, please open an issue on GitHub or contact us at tunahan.ali.ozturk@outlook.com

❤️ Acknowledgements

Special thanks to the .NET community for inspiration and support in building this library..


👨‍💻 About the Author

Moongazing Pipeline Enhancer is developed and maintained by the Tunahan Ali Ozturk, passionate software developer
dedicated to creating scalable and maintainable solutions.

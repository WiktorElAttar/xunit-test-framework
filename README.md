# XunitTestFramework

A comprehensive integration testing framework for .NET 8 using xUnit v2, designed to help developers write integration tests by abstracting infrastructure setup and dependency management.

## Features

- **Service Integration Testing**: Test individual services with dependency injection and mocked external dependencies
- **API Integration Testing**: Test HTTP endpoints with a test server and full request/response cycle
- **Flexible Configuration**: Easy service configuration and replacement for testing scenarios
- **Async Support**: Full async/await support for modern .NET applications
- **xUnit Integration**: Seamless integration with xUnit v2 testing framework
- **Logging Support**: Built-in logging configuration for debugging tests
- **Abstract Base Classes**: Project-specific implementations for maximum flexibility

## Installation

Add the package to your test project:

```xml
<PackageReference Include="XunitTestFramework" Version="1.0.0" />
```

## Quick Start

### Service Integration Testing

Create a test fixture for your service:

```csharp
public class MyServiceTestFixture : ServiceIntegrationTestFixture
{
    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Register your services
            services.AddScoped<IMyService, MyService>();
            services.AddScoped<IMyRepository, MyRepository>();
            
            // Replace external dependencies with mocks
            services.Replace<IExternalService>(new MockExternalService());
        });
    }
}
```

Write your test:

```csharp
public class MyServiceTests : ServiceIntegrationTestBase<MyServiceTestFixture>
{
    public MyServiceTests(MyServiceTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Should_Process_Data_Correctly()
    {
        // Arrange
        var service = GetRequiredService<IMyService>();
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await service.ProcessAsync(testData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Processed: Test", result.Message);
    }
}
```

### API Integration Testing

Create a test fixture for your API:

```csharp
public class MyApiTestFixture : ApiIntegrationTestFixture
{
    protected override void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.ConfigureServices(services =>
            {
                services.AddControllers();
                services.AddScoped<IMyService, MyService>();
            });
        });
    }
}
```

Write your API test:

```csharp
public class MyApiTests : ApiIntegrationTestBase<MyApiTestFixture>
{
    public MyApiTests(MyApiTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Get_Endpoint_Should_Return_Data()
    {
        // Arrange
        var endpoint = "/api/data";

        // Act
        var response = await GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var data = await DeserializeResponseAsync<List<DataModel>>(response);
        Assert.NotNull(data);
        Assert.NotEmpty(data);
    }

    [Fact]
    public async Task Post_Endpoint_Should_Create_Resource()
    {
        // Arrange
        var endpoint = "/api/data";
        var newData = new DataModel { Name = "New Item" };
        var content = SerializeToJsonContent(newData);

        // Act
        var response = await PostAsync(endpoint, content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdData = await DeserializeResponseAsync<DataModel>(response);
        Assert.NotNull(createdData);
        Assert.Equal("New Item", createdData.Name);
    }
}
```

## Project-Specific Implementations

The framework provides abstract base classes that allow each test project to implement their own specific versions. This approach gives you maximum flexibility and control over your test setup.

### Creating Project-Specific Base Classes

Instead of inheriting directly from the framework's base classes, create your own project-specific implementations that automatically handle fixture injection:

```csharp
// Service test base for your project
public abstract class ServiceTestBase : IClassFixture<MyServiceTestFixture>, IAsyncLifetime
{
    protected MyServiceTestFixture Fixture { get; }
    protected IServiceProvider ServiceProvider => Fixture.ServiceProvider;
    protected ILoggerFactory LoggerFactory => Fixture.LoggerFactory;

    protected ServiceTestBase(MyServiceTestFixture fixture)
    {
        Fixture = fixture;
    }

    // Service access methods
    protected T GetService<T>() where T : class => Fixture.GetService<T>();
    protected T GetRequiredService<T>() where T : class => Fixture.GetRequiredService<T>();
    protected IServiceScope CreateScope() => Fixture.CreateScope();

    // Service configuration methods
    protected void ConfigureServices(Action<IServiceCollection> configureServices) 
        => Fixture.ConfigureServices(configureServices);
    protected void ReplaceService<TService, TImplementation>() 
        where TService : class 
        where TImplementation : class, TService 
        => Fixture.ReplaceService<TService, TImplementation>();
    protected void ReplaceService<TService>(TService implementation) 
        where TService : class 
        => Fixture.ReplaceService(implementation);

    // Lifecycle methods
    public virtual async Task InitializeAsync() => await Fixture.InitializeAsync();
    public virtual async Task DisposeAsync() => await Fixture.ResetAsync();
}

// API test base for your project
public abstract class ApiTestBase : IClassFixture<MyApiTestFixture>, IAsyncLifetime
{
    protected MyApiTestFixture Fixture { get; }
    protected IServiceProvider ServiceProvider => Fixture.ServiceProvider;
    protected ILoggerFactory LoggerFactory => Fixture.LoggerFactory;
    protected HttpClient HttpClient => Fixture.HttpClient;

    protected ApiTestBase(MyApiTestFixture fixture)
    {
        Fixture = fixture;
    }

    // HTTP client methods
    protected HttpClient CreateClient(Action<HttpClient>? configureClient = null) 
        => Fixture.CreateClient(configureClient);

    // HTTP request methods
    protected async Task<HttpResponseMessage> GetAsync(string endpoint, IDictionary<string, string>? headers = null) 
        => await Fixture.GetAsync(endpoint, headers);
    protected async Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent? content = null, IDictionary<string, string>? headers = null) 
        => await Fixture.PostAsync(endpoint, content, headers);
    protected async Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent? content = null, IDictionary<string, string>? headers = null) 
        => await Fixture.PutAsync(endpoint, content, headers);
    protected async Task<HttpResponseMessage> DeleteAsync(string endpoint, IDictionary<string, string>? headers = null) 
        => await Fixture.DeleteAsync(endpoint, headers);
    protected async Task<HttpResponseMessage> PatchAsync(string endpoint, HttpContent? content = null, IDictionary<string, string>? headers = null) 
        => await Fixture.PatchAsync(endpoint, content, headers);

    // Serialization methods
    protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response) 
        => await Fixture.DeserializeResponseAsync<T>(response);
    protected HttpContent SerializeToJsonContent<T>(T obj) 
        => Fixture.SerializeToJsonContent(obj);

    // Lifecycle methods
    public virtual async Task InitializeAsync() => await Fixture.InitializeAsync();
    public virtual async Task DisposeAsync() => await Fixture.ResetAsync();

    // Project-specific helper methods
    protected void AssertSuccess(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, 
            $"Expected success status code, but got {response.StatusCode}. Content: {response.Content.ReadAsStringAsync().Result}");
    }

    protected void AssertStatusCode(HttpResponseMessage response, HttpStatusCode expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }
}
```

### Using Project-Specific Base Classes

Now your tests inherit from your project-specific base classes with automatic fixture injection:

```csharp
// Service tests - clean and simple!
public class UserServiceTests : ServiceTestBase
{
    public UserServiceTests(MyServiceTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Should_Get_User_By_Id()
    {
        // Arrange
        var service = GetRequiredService<IUserService>();
        
        // Act
        var user = await service.GetUserAsync(1);
        
        // Assert
        Assert.NotNull(user);
    }
}

// API tests - clean and simple!
public class UserApiTests : ApiTestBase
{
    public UserApiTests(MyApiTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Get_User_Should_Return_Success()
    {
        // Act
        var response = await GetAsync("/api/users/1");
        
        // Assert
        AssertSuccess(response); // Using project-specific helper method
    }
}
```

### Benefits of This Approach

1. **Project-Specific Logic**: Add initialization, cleanup, and helper methods specific to your project
2. **Consistent Patterns**: Enforce consistent testing patterns across your project
3. **Easier Maintenance**: Centralize common test logic in one place
4. **Better Organization**: Keep project-specific concerns separate from framework concerns
5. **Flexibility**: Each project can have its own implementation without affecting others
6. **Clean Test Classes**: Test classes are focused on test logic, not infrastructure setup
7. **Automatic Fixture Injection**: xUnit automatically handles fixture lifecycle and injection

## Advanced Usage

### Service Replacement

Replace services with mocks or test implementations:

```csharp
// Replace with a specific instance
var mockService = new Mock<IMyService>();
mockService.Setup(x => x.GetDataAsync()).ReturnsAsync(testData);
ReplaceService(mockService.Object);

// Replace with a different implementation
ReplaceService<IMyService, TestMyService>();
```

### Custom Configuration

Add custom configuration for your tests:

```csharp
protected override IDictionary<string, string?> GetTestConfiguration()
{
    return new Dictionary<string, string?>
    {
        ["ConnectionStrings:Default"] = "Server=localhost;Database=TestDb;",
        ["FeatureFlags:NewFeature"] = "true",
        ["Logging:LogLevel:Default"] = "Debug"
    };
}
```

### HTTP Client Customization

Create custom HTTP clients for specific test scenarios:

```csharp
var client = CreateClient(httpClient =>
{
    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
    httpClient.Timeout = TimeSpan.FromSeconds(30);
});
```

## Extension Methods

The framework provides useful extension methods for service configuration:

```csharp
services.AddTestLogging(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
});

services.Replace<IMyService, TestMyService>();
```

## Best Practices

1. **Use Test Fixtures**: Create dedicated test fixtures for different test scenarios
2. **Mock External Dependencies**: Replace external services with mocks or test implementations
3. **Clean Up Resources**: The framework automatically handles cleanup, but ensure your tests don't leave side effects
4. **Use Async/Await**: Always use async/await for asynchronous operations
5. **Test Isolation**: Each test should be independent and not rely on the state of other tests

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
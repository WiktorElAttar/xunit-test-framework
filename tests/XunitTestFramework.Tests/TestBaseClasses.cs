using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using XunitTestFramework.Base;
using XunitTestFramework.Interfaces;

namespace XunitTestFramework.Tests;

/// <summary>
/// Specific implementation of ServiceIntegrationTestBase for this test project.
/// Automatically handles fixture injection through IClassFixture interface.
/// </summary>
public abstract class ServiceTestBase : IClassFixture<UserServiceTestFixture>, IAsyncLifetime
{
    /// <summary>
    /// Gets the test fixture instance.
    /// </summary>
    protected UserServiceTestFixture Fixture { get; }

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    protected IServiceProvider ServiceProvider => Fixture.ServiceProvider;

    /// <summary>
    /// Gets the logger factory for creating loggers.
    /// </summary>
    protected ILoggerFactory LoggerFactory => Fixture.LoggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTestBase"/> class.
    /// </summary>
    /// <param name="fixture">The test fixture instance (automatically injected by xUnit).</param>
    protected ServiceTestBase(UserServiceTestFixture fixture)
    {
        Fixture = fixture;
    }

    /// <summary>
    /// Gets a service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>The service instance.</returns>
    protected T GetService<T>() where T : class
    {
        return Fixture.GetService<T>();
    }

    /// <summary>
    /// Gets a required service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>The service instance.</returns>
    protected T GetRequiredService<T>() where T : class
    {
        return Fixture.GetRequiredService<T>();
    }

    /// <summary>
    /// Creates a new scope for the service provider.
    /// </summary>
    /// <returns>A new service scope.</returns>
    protected IServiceScope CreateScope()
    {
        return Fixture.CreateScope();
    }

    /// <summary>
    /// Configures services for the test fixture.
    /// </summary>
    /// <param name="configureServices">Action to configure services.</param>
    protected void ConfigureServices(Action<IServiceCollection> configureServices)
    {
        Fixture.ConfigureServices(configureServices);
    }

    /// <summary>
    /// Replaces a service registration with a mock or test implementation.
    /// </summary>
    /// <typeparam name="TService">The service type to replace.</typeparam>
    /// <typeparam name="TImplementation">The implementation type to use.</typeparam>
    protected void ReplaceService<TService, TImplementation>() 
        where TService : class 
        where TImplementation : class, TService
    {
        Fixture.ReplaceService<TService, TImplementation>();
    }

    /// <summary>
    /// Replaces a service registration with a specific instance.
    /// </summary>
    /// <typeparam name="TService">The service type to replace.</typeparam>
    /// <param name="implementation">The implementation instance to use.</param>
    protected void ReplaceService<TService>(TService implementation) where TService : class
    {
        Fixture.ReplaceService(implementation);
    }

    /// <summary>
    /// Called before each test to set up the test environment.
    /// </summary>
    /// <returns>A task that represents the asynchronous setup operation.</returns>
    public virtual async Task InitializeAsync()
    {
        await Fixture.InitializeAsync();
    }

    /// <summary>
    /// Called after each test to clean up the test environment.
    /// </summary>
    /// <returns>A task that represents the asynchronous cleanup operation.</returns>
    public virtual async Task DisposeAsync()
    {
        await Fixture.ResetAsync();
    }
}

/// <summary>
/// Specific implementation of ApiIntegrationTestBase for this test project.
/// Automatically handles fixture injection through IClassFixture interface.
/// </summary>
public abstract class ApiTestBase : IClassFixture<TestWebAppFixture>, IAsyncLifetime
{
    /// <summary>
    /// Gets the test fixture instance.
    /// </summary>
    protected TestWebAppFixture Fixture { get; }

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    protected IServiceProvider ServiceProvider => Fixture.ServiceProvider;

    /// <summary>
    /// Gets the logger factory for creating loggers.
    /// </summary>
    protected ILoggerFactory LoggerFactory => Fixture.LoggerFactory;

    /// <summary>
    /// Gets the HTTP client for making requests to the test server.
    /// </summary>
    protected HttpClient HttpClient => Fixture.HttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiTestBase"/> class.
    /// </summary>
    /// <param name="fixture">The test fixture instance (automatically injected by xUnit).</param>
    protected ApiTestBase(TestWebAppFixture fixture)
    {
        Fixture = fixture;
    }

    /// <summary>
    /// Creates a new HTTP client with custom configuration.
    /// </summary>
    /// <param name="configureClient">Action to configure the HTTP client.</param>
    /// <returns>A new HTTP client instance.</returns>
    protected HttpClient CreateClient(Action<HttpClient>? configureClient = null)
    {
        return Fixture.CreateClient(configureClient);
    }

    /// <summary>
    /// Sends an HTTP GET request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    protected async Task<HttpResponseMessage> GetAsync(string endpoint, IDictionary<string, string>? headers = null)
    {
        return await Fixture.GetAsync(endpoint, headers);
    }

    /// <summary>
    /// Sends an HTTP POST request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="content">The content to send in the request body.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    protected async Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent? content = null, IDictionary<string, string>? headers = null)
    {
        return await Fixture.PostAsync(endpoint, content, headers);
    }

    /// <summary>
    /// Sends an HTTP PUT request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="content">The content to send in the request body.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    protected async Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent? content = null, IDictionary<string, string>? headers = null)
    {
        return await Fixture.PutAsync(endpoint, content, headers);
    }

    /// <summary>
    /// Sends an HTTP DELETE request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    protected async Task<HttpResponseMessage> DeleteAsync(string endpoint, IDictionary<string, string>? headers = null)
    {
        return await Fixture.DeleteAsync(endpoint, headers);
    }

    /// <summary>
    /// Sends an HTTP PATCH request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="content">The content to send in the request body.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    protected async Task<HttpResponseMessage> PatchAsync(string endpoint, HttpContent? content = null, IDictionary<string, string>? headers = null)
    {
        return await Fixture.PatchAsync(endpoint, content, headers);
    }

    /// <summary>
    /// Deserializes the response content to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <returns>The deserialized object.</returns>
    protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        return await Fixture.DeserializeResponseAsync<T>(response);
    }

    /// <summary>
    /// Serializes an object to JSON content for HTTP requests.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>The JSON content.</returns>
    protected HttpContent SerializeToJsonContent<T>(T obj)
    {
        return Fixture.SerializeToJsonContent(obj);
    }

    /// <summary>
    /// Called before each test to set up the test environment.
    /// </summary>
    /// <returns>A task that represents the asynchronous setup operation.</returns>
    public virtual async Task InitializeAsync()
    {
        await Fixture.InitializeAsync();
    }

    /// <summary>
    /// Called after each test to clean up the test environment.
    /// </summary>
    /// <returns>A task that represents the asynchronous cleanup operation.</returns>
    public virtual async Task DisposeAsync()
    {
        await Fixture.ResetAsync();
    }

    /// <summary>
    /// Helper method to assert that a response is successful.
    /// </summary>
    /// <param name="response">The HTTP response to check.</param>
    protected void AssertSuccess(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, 
            $"Expected success status code, but got {response.StatusCode}. Content: {response.Content.ReadAsStringAsync().Result}");
    }

    /// <summary>
    /// Helper method to assert that a response has a specific status code.
    /// </summary>
    /// <param name="response">The HTTP response to check.</param>
    /// <param name="expectedStatusCode">The expected status code.</param>
    protected void AssertStatusCode(HttpResponseMessage response, HttpStatusCode expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }
} 
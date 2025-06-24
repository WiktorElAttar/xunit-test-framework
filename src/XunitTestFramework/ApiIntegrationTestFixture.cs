using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XunitTestFramework.Interfaces;

namespace XunitTestFramework;

/// <summary>
/// API integration test fixture for testing HTTP endpoints with a test server
/// using WebApplicationFactory following Microsoft's official pattern.
/// </summary>
/// <typeparam name="TEntryPoint">The entry point class of the SUT (usually Program.cs).</typeparam>
public class ApiIntegrationTestFixture<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IApiIntegrationTestFixture
    where TEntryPoint : class
{
    private readonly List<Action<IServiceCollection>> _serviceConfigurations = new();
    private readonly List<Action<IWebHostBuilder>> _webHostConfigurations = new();

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public IServiceProvider ServiceProvider => Services;

    /// <summary>
    /// Gets the configuration for the test environment.
    /// </summary>
    public IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

    /// <summary>
    /// Gets the logger factory for creating loggers during tests.
    /// </summary>
    public ILoggerFactory LoggerFactory => Services.GetRequiredService<ILoggerFactory>();

    /// <summary>
    /// Gets the test server instance for making HTTP requests.
    /// </summary>
    public TestServer TestServer => Server;

    /// <summary>
    /// Gets the HTTP client for making requests to the test server.
    /// </summary>
    public HttpClient HttpClient => CreateClient();

    /// <summary>
    /// Creates a new HTTP client with custom configuration.
    /// </summary>
    /// <param name="configureClient">Action to configure the HTTP client.</param>
    /// <returns>A new HTTP client instance.</returns>
    public HttpClient CreateClient(Action<HttpClient>? configureClient = null)
    {
        var client = base.CreateClient();
        configureClient?.Invoke(client);
        return client;
    }

    /// <summary>
    /// Sends an HTTP GET request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    public async Task<HttpResponseMessage> GetAsync(string endpoint, IDictionary<string, string>? headers = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddHeaders(request, headers);
        return await HttpClient.SendAsync(request);
    }

    /// <summary>
    /// Sends an HTTP POST request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="content">The content to send in the request body.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    public async Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent? content = null, IDictionary<string, string>? headers = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
        AddHeaders(request, headers);
        return await HttpClient.SendAsync(request);
    }

    /// <summary>
    /// Sends an HTTP PUT request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="content">The content to send in the request body.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    public async Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent? content = null, IDictionary<string, string>? headers = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, endpoint) { Content = content };
        AddHeaders(request, headers);
        return await HttpClient.SendAsync(request);
    }

    /// <summary>
    /// Sends an HTTP DELETE request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    public async Task<HttpResponseMessage> DeleteAsync(string endpoint, IDictionary<string, string>? headers = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        AddHeaders(request, headers);
        return await HttpClient.SendAsync(request);
    }

    /// <summary>
    /// Sends an HTTP PATCH request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="content">The content to send in the request body.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    public async Task<HttpResponseMessage> PatchAsync(string endpoint, HttpContent? content = null, IDictionary<string, string>? headers = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
        AddHeaders(request, headers);
        return await HttpClient.SendAsync(request);
    }

    /// <summary>
    /// Deserializes the response content to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <returns>The deserialized object.</returns>
    public async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    /// <summary>
    /// Serializes an object to JSON content for HTTP requests.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>The JSON content.</returns>
    public HttpContent SerializeToJsonContent<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Configures services for the test fixture.
    /// </summary>
    /// <param name="configureServices">Action to configure services.</param>
    public void ConfigureServices(Action<IServiceCollection> configureServices)
    {
        _serviceConfigurations.Add(configureServices);
    }

    /// <summary>
    /// Configures the web host builder for the test fixture.
    /// </summary>
    /// <param name="configureWebHost">Action to configure the web host.</param>
    public void ConfigureWebHost(Action<IWebHostBuilder> configureWebHost)
    {
        _webHostConfigurations.Add(configureWebHost);
    }

    /// <summary>
    /// Initializes the test fixture asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public Task InitializeAsync()
    {
        // WebApplicationFactory is already initialized when created
        return Task.CompletedTask;
    }

    /// <summary>
    /// Resets the test fixture to a clean state between tests.
    /// </summary>
    /// <returns>A task that represents the asynchronous reset operation.</returns>
    public Task ResetAsync()
    {
        // WebApplicationFactory maintains state, so we just return completed task
        // Individual tests should handle their own cleanup
        return Task.CompletedTask;
    }

    /// <summary>
    /// Configures the web host builder with custom services and settings.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Apply all service configurations
            foreach (var configuration in _serviceConfigurations)
            {
                configuration(services);
            }
        });

        // Apply all web host configurations
        foreach (var configuration in _webHostConfigurations)
        {
            configuration(builder);
        }

        // Set test environment
        builder.UseEnvironment("Test");
    }

    /// <summary>
    /// Disposes of the test fixture and its resources.
    /// </summary>
    public new async ValueTask DisposeAsync()
    {
        Dispose();
        await ValueTask.CompletedTask;
    }

    private static void AddHeaders(HttpRequestMessage request, IDictionary<string, string>? headers)
    {
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }
    }
} 
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using XunitTestFramework.Attributes;
using XunitTestFramework.Interfaces;

namespace XunitTestFramework.Base;

/// <summary>
/// Abstract base class for API integration tests that provides common functionality
/// for testing HTTP endpoints with a test server using WebApplicationFactory.
/// </summary>
/// <typeparam name="TEntryPoint">The entry point class of the SUT (usually Program.cs).</typeparam>
/// <typeparam name="TFixture">The type of the test fixture.</typeparam>
public abstract class ApiIntegrationTestBase<TEntryPoint, TFixture> : IClassFixture<TFixture>, IAsyncLifetime
    where TEntryPoint : class
    where TFixture : class, IApiIntegrationTestFixture
{
    /// <summary>
    /// Gets the test fixture instance.
    /// </summary>
    protected TFixture Fixture { get; }

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
    /// Initializes a new instance of the <see cref="ApiIntegrationTestBase{TEntryPoint, TFixture}"/> class.
    /// </summary>
    /// <param name="fixture">The test fixture instance.</param>
    protected ApiIntegrationTestBase(TFixture fixture)
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
} 
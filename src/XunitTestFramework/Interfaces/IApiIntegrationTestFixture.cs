using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace XunitTestFramework.Interfaces;

/// <summary>
/// Interface for API integration test fixtures that focus on testing HTTP endpoints
/// with a test server and full request/response cycle.
/// </summary>
public interface IApiIntegrationTestFixture : IIntegrationTestFixture
{
    /// <summary>
    /// Gets the test server instance for making HTTP requests.
    /// </summary>
    TestServer TestServer { get; }
    
    /// <summary>
    /// Gets the HTTP client for making requests to the test server.
    /// </summary>
    HttpClient HttpClient { get; }
    
    /// <summary>
    /// Creates a new HTTP client with custom configuration.
    /// </summary>
    /// <param name="configureClient">Action to configure the HTTP client.</param>
    /// <returns>A new HTTP client instance.</returns>
    HttpClient CreateClient(Action<HttpClient>? configureClient = null);
    
    /// <summary>
    /// Sends an HTTP GET request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    Task<HttpResponseMessage> GetAsync(string endpoint, IDictionary<string, string>? headers = null);
    
    /// <summary>
    /// Sends an HTTP POST request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="content">The content to send in the request body.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent? content = null, IDictionary<string, string>? headers = null);
    
    /// <summary>
    /// Sends an HTTP PUT request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="content">The content to send in the request body.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent? content = null, IDictionary<string, string>? headers = null);
    
    /// <summary>
    /// Sends an HTTP DELETE request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    Task<HttpResponseMessage> DeleteAsync(string endpoint, IDictionary<string, string>? headers = null);
    
    /// <summary>
    /// Sends an HTTP PATCH request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="content">The content to send in the request body.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <returns>The HTTP response message.</returns>
    Task<HttpResponseMessage> PatchAsync(string endpoint, HttpContent? content = null, IDictionary<string, string>? headers = null);
    
    /// <summary>
    /// Deserializes the response content to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <returns>The deserialized object.</returns>
    Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response);
    
    /// <summary>
    /// Serializes an object to JSON content for HTTP requests.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>The JSON content.</returns>
    HttpContent SerializeToJsonContent<T>(T obj);
} 
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace XunitTestFramework.Interfaces;

/// <summary>
/// Base interface for integration test fixtures that provides common functionality
/// for setting up test infrastructure and managing test lifecycle.
/// </summary>
public interface IIntegrationTestFixture : IAsyncDisposable
{
    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    IServiceProvider ServiceProvider { get; }
    
    /// <summary>
    /// Gets the configuration for the test environment.
    /// </summary>
    IConfiguration Configuration { get; }
    
    /// <summary>
    /// Gets the logger factory for creating loggers during tests.
    /// </summary>
    ILoggerFactory LoggerFactory { get; }
    
    /// <summary>
    /// Initializes the test fixture asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    Task InitializeAsync();
    
    /// <summary>
    /// Resets the test fixture to a clean state between tests.
    /// </summary>
    /// <returns>A task that represents the asynchronous reset operation.</returns>
    Task ResetAsync();
} 
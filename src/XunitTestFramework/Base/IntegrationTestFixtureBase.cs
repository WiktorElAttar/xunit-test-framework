using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XunitTestFramework.Interfaces;

namespace XunitTestFramework.Base;

/// <summary>
/// Base implementation for integration test fixtures that provides common functionality
/// for setting up test infrastructure and managing test lifecycle.
/// </summary>
public abstract class IntegrationTestFixtureBase : IIntegrationTestFixture
{
    private IHost? _host;
    private bool _disposed;

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public IServiceProvider ServiceProvider => _host?.Services ?? throw new InvalidOperationException("Test fixture not initialized. Call InitializeAsync() first.");

    /// <summary>
    /// Gets the configuration for the test environment.
    /// </summary>
    public IConfiguration Configuration => _host?.Services.GetRequiredService<IConfiguration>() ?? throw new InvalidOperationException("Test fixture not initialized. Call InitializeAsync() first.");

    /// <summary>
    /// Gets the logger factory for creating loggers during tests.
    /// </summary>
    public ILoggerFactory LoggerFactory => _host?.Services.GetRequiredService<ILoggerFactory>() ?? throw new InvalidOperationException("Test fixture not initialized. Call InitializeAsync() first.");

    /// <summary>
    /// Initializes the test fixture asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public virtual async Task InitializeAsync()
    {
        if (_host != null)
        {
            return;
        }

        var builder = CreateHostBuilder();
        ConfigureHost(builder);
        _host = await builder.StartAsync();
    }

    /// <summary>
    /// Resets the test fixture to a clean state between tests.
    /// </summary>
    /// <returns>A task that represents the asynchronous reset operation.</returns>
    public virtual async Task ResetAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
            _host = null;
        }

        await InitializeAsync();
    }

    /// <summary>
    /// Creates the host builder for the test fixture.
    /// </summary>
    /// <returns>The host builder instance.</returns>
    protected virtual IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: true)
                      .AddJsonFile("appsettings.Testing.json", optional: true)
                      .AddEnvironmentVariables()
                      .AddInMemoryCollection(GetTestConfiguration());
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Debug);
            });
    }

    /// <summary>
    /// Configures the host builder with additional services and settings.
    /// </summary>
    /// <param name="builder">The host builder to configure.</param>
    protected abstract void ConfigureHost(IHostBuilder builder);

    /// <summary>
    /// Gets additional configuration values for testing.
    /// </summary>
    /// <returns>A dictionary of configuration key-value pairs.</returns>
    protected virtual IDictionary<string, string?> GetTestConfiguration()
    {
        return new Dictionary<string, string?>
        {
            ["Logging:LogLevel:Default"] = "Debug",
            ["Logging:LogLevel:Microsoft"] = "Warning",
            ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Information"
        };
    }

    /// <summary>
    /// Disposes of the test fixture and its resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        _disposed = true;
    }
} 
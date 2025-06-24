using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;
using XunitTestFramework.Attributes;
using XunitTestFramework.Interfaces;

namespace XunitTestFramework.Base;

/// <summary>
/// Abstract base class for service integration tests that provides common functionality
/// for testing individual services with dependency injection.
/// </summary>
/// <typeparam name="TFixture">The type of the test fixture.</typeparam>
public abstract class ServiceIntegrationTestBase<TFixture> : IClassFixture<TFixture>, IAsyncLifetime
    where TFixture : class, IServiceIntegrationTestFixture
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
    /// Initializes a new instance of the <see cref="ServiceIntegrationTestBase{TFixture}"/> class.
    /// </summary>
    /// <param name="fixture">The test fixture instance.</param>
    protected ServiceIntegrationTestBase(TFixture fixture)
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
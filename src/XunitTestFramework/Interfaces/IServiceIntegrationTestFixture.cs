using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace XunitTestFramework.Interfaces;

/// <summary>
/// Interface for service integration test fixtures that focus on testing individual services
/// with dependency injection and mocked external dependencies.
/// </summary>
public interface IServiceIntegrationTestFixture : IIntegrationTestFixture
{
    /// <summary>
    /// Gets the service collection for configuring services.
    /// </summary>
    IServiceCollection Services { get; }
    
    /// <summary>
    /// Gets a service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>The service instance.</returns>
    T GetService<T>() where T : class;
    
    /// <summary>
    /// Gets a required service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>The service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service is not registered.</exception>
    T GetRequiredService<T>() where T : class;
    
    /// <summary>
    /// Creates a new scope for the service provider.
    /// </summary>
    /// <returns>A new service scope.</returns>
    IServiceScope CreateScope();
    
    /// <summary>
    /// Configures services for the test fixture.
    /// </summary>
    /// <param name="configureServices">Action to configure services.</param>
    void ConfigureServices(Action<IServiceCollection> configureServices);
    
    /// <summary>
    /// Replaces a service registration with a mock or test implementation.
    /// </summary>
    /// <typeparam name="TService">The service type to replace.</typeparam>
    /// <typeparam name="TImplementation">The implementation type to use.</typeparam>
    void ReplaceService<TService, TImplementation>() 
        where TService : class 
        where TImplementation : class, TService;
    
    /// <summary>
    /// Replaces a service registration with a specific instance.
    /// </summary>
    /// <typeparam name="TService">The service type to replace.</typeparam>
    /// <param name="implementation">The implementation instance to use.</param>
    void ReplaceService<TService>(TService implementation) where TService : class;
} 
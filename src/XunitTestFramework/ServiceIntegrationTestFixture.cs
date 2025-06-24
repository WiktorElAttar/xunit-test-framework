using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XunitTestFramework.Base;
using XunitTestFramework.Interfaces;

namespace XunitTestFramework;

/// <summary>
/// Service integration test fixture for testing individual services with dependency injection
/// and mocked external dependencies.
/// </summary>
public class ServiceIntegrationTestFixture : IntegrationTestFixtureBase, IServiceIntegrationTestFixture
{
    private readonly List<Action<IServiceCollection>> _serviceConfigurations = new();
    private readonly Dictionary<Type, object> _serviceReplacements = new();

    /// <summary>
    /// Gets the service collection for configuring services.
    /// </summary>
    public IServiceCollection Services { get; private set; } = new ServiceCollection();

    /// <summary>
    /// Gets a service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>The service instance.</returns>
    public T GetService<T>() where T : class
    {
        return ServiceProvider.GetService<T>() ?? throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
    }

    /// <summary>
    /// Gets a required service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>The service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service is not registered.</exception>
    public T GetRequiredService<T>() where T : class
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Creates a new scope for the service provider.
    /// </summary>
    /// <returns>A new service scope.</returns>
    public IServiceScope CreateScope()
    {
        return ServiceProvider.CreateScope();
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
    /// Replaces a service registration with a mock or test implementation.
    /// </summary>
    /// <typeparam name="TService">The service type to replace.</typeparam>
    /// <typeparam name="TImplementation">The implementation type to use.</typeparam>
    public void ReplaceService<TService, TImplementation>() 
        where TService : class 
        where TImplementation : class, TService
    {
        _serviceReplacements[typeof(TService)] = typeof(TImplementation);
    }

    /// <summary>
    /// Replaces a service registration with a specific instance.
    /// </summary>
    /// <typeparam name="TService">The service type to replace.</typeparam>
    /// <param name="implementation">The implementation instance to use.</param>
    public void ReplaceService<TService>(TService implementation) where TService : class
    {
        _serviceReplacements[typeof(TService)] = implementation;
    }

    /// <summary>
    /// Configures the host builder with service collection and dependency injection.
    /// </summary>
    /// <param name="builder">The host builder to configure.</param>
    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            // Add configuration and logging services
            services.AddSingleton(context.Configuration);
            services.AddLogging();

            // Apply all service configurations
            foreach (var configuration in _serviceConfigurations)
            {
                configuration(services);
            }

            // Apply service replacements
            foreach (var replacement in _serviceReplacements)
            {
                if (replacement.Value is Type implementationType)
                {
                    // Replace with new implementation type
                    var descriptor = services.FirstOrDefault(s => s.ServiceType == replacement.Key);
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                        services.Add(new ServiceDescriptor(replacement.Key, implementationType, descriptor.Lifetime));
                    }
                }
                else
                {
                    // Replace with specific instance
                    services.AddSingleton(replacement.Key, replacement.Value);
                }
            }

            Services = services;
        });
    }

    /// <summary>
    /// Resets the test fixture to a clean state between tests.
    /// </summary>
    /// <returns>A task that represents the asynchronous reset operation.</returns>
    public override async Task ResetAsync()
    {
        // Clear service configurations and replacements
        _serviceConfigurations.Clear();
        _serviceReplacements.Clear();
        Services = new ServiceCollection();

        await base.ResetAsync();
    }
} 
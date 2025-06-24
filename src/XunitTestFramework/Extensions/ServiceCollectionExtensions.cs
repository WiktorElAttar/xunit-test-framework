using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace XunitTestFramework.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to simplify service configuration in tests.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Replaces a service registration with a new implementation.
    /// </summary>
    /// <typeparam name="TService">The type of the service to replace.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="implementation">The new implementation.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection Replace<TService>(
        this IServiceCollection services,
        TService implementation)
        where TService : class
    {
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
        return services.AddSingleton(implementation);
    }

    /// <summary>
    /// Replaces a service registration with a new implementation type.
    /// </summary>
    /// <typeparam name="TService">The type of the service to replace.</typeparam>
    /// <typeparam name="TImplementation">The new implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection Replace<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
            services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), descriptor.Lifetime));
        }
        return services;
    }

    /// <summary>
    /// Adds logging with console and debug providers for testing.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureLogging">Optional action to configure logging.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddTestLogging(
        this IServiceCollection services,
        Action<ILoggingBuilder>? configureLogging = null)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Debug);
            configureLogging?.Invoke(builder);
        });

        return services;
    }
} 
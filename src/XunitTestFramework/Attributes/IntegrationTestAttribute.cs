namespace XunitTestFramework.Attributes;

/// <summary>
/// Custom attribute for marking integration tests that provides additional metadata
/// and can be used for test discovery and filtering.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class IntegrationTestAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the test category.
    /// </summary>
    public string Category { get; set; } = "Integration";

    /// <summary>
    /// Gets or sets the test environment.
    /// </summary>
    public string Environment { get; set; } = "Test";

    /// <summary>
    /// Gets or sets the test timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTestAttribute"/> class.
    /// </summary>
    public IntegrationTestAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTestAttribute"/> class with a specific category.
    /// </summary>
    /// <param name="category">The test category.</param>
    public IntegrationTestAttribute(string category)
    {
        Category = category;
    }
} 
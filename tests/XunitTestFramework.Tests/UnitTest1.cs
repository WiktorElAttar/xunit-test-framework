using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;
using XunitTestFramework;
using XunitTestFramework.Base;
using XunitTestFramework.Interfaces;

namespace XunitTestFramework.Tests;

// Example service interfaces and implementations for testing
public interface IUserService
{
    Task<User> GetUserAsync(int id);
    Task<User> CreateUserAsync(User user);
}

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(User user);
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public UserService(IUserRepository userRepository, IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<User> GetUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new ArgumentException($"User with id {id} not found");
        return user;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        var createdUser = await _userRepository.CreateAsync(user);
        
        // Send welcome email
        await _emailService.SendEmailAsync(createdUser.Email, "Welcome!", "Welcome to our platform!");
        
        return createdUser;
    }
}

// Service Integration Test Fixture
public class UserServiceTestFixture : ServiceIntegrationTestFixture
{
    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();
            
            // Use NSubstitute to create mocks
            var emailService = Substitute.For<IEmailService>();
            services.AddScoped<IEmailService>(_ => emailService);
        });
    }
}

// Test fixture for testing with specific mocks
public class UserServiceWithMocksTestFixture : ServiceIntegrationTestFixture
{
    public IEmailService EmailService { get; private set; } = Substitute.For<IEmailService>();
    public IUserRepository UserRepository { get; private set; } = Substitute.For<IUserRepository>();

    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository>(_ => UserRepository);
            services.AddScoped<IEmailService>(_ => EmailService);
        });
    }
}

// Simple repository implementation for testing
public class UserRepository : IUserRepository
{
    private readonly Dictionary<int, User> _users = new();
    private int _nextId = 1;

    public Task<User?> GetByIdAsync(int id)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User> CreateAsync(User user)
    {
        user.Id = _nextId++;
        _users[user.Id] = user;
        return Task.FromResult(user);
    }
}

// Service Integration Tests using specific base class
public class UserServiceTests : ServiceTestBase
{
    public UserServiceTests(UserServiceTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetUserAsync_Should_Return_User()
    {
        // Arrange
        var userService = GetRequiredService<IUserService>();
        var userRepository = GetRequiredService<IUserRepository>();
        var emailService = GetRequiredService<IEmailService>();
        
        var testUser = new User { Id = 1, Name = "John Doe", Email = "john@example.com" };
        await userRepository.CreateAsync(testUser);

        // Act
        var user = await userService.GetUserAsync(1);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(1, user.Id);
        Assert.Equal("John Doe", user.Name);
        Assert.Equal("john@example.com", user.Email);
    }

    [Fact]
    public async Task GetUserAsync_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        var userService = GetRequiredService<IUserService>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.GetUserAsync(999));
    }

    [Fact]
    public async Task CreateUserAsync_Should_Create_User_And_Send_Email()
    {
        // Arrange
        var userService = GetRequiredService<IUserService>();
        var emailService = GetRequiredService<IEmailService>();
        var newUser = new User { Name = "Jane Doe", Email = "jane@example.com" };

        // Act
        var createdUser = await userService.CreateUserAsync(newUser);

        // Assert
        Assert.NotNull(createdUser);
        Assert.NotEqual(0, createdUser.Id);
        Assert.Equal("Jane Doe", createdUser.Name);
        Assert.Equal("jane@example.com", createdUser.Email);
        
        // Verify email was sent using NSubstitute
        await emailService.Received(1).SendEmailAsync("jane@example.com", "Welcome!", "Welcome to our platform!");
    }
}

// Separate test class for tests that need specific mock configurations
public class UserServiceWithMocksTests : IClassFixture<UserServiceWithMocksTestFixture>, IAsyncLifetime
{
    private readonly UserServiceWithMocksTestFixture _fixture;
    private IUserService _userService = null!;

    public UserServiceWithMocksTests(UserServiceWithMocksTestFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.InitializeAsync();
        _userService = _fixture.GetRequiredService<IUserService>();
    }

    public async Task DisposeAsync()
    {
        await _fixture.ResetAsync();
    }

    [Fact]
    public async Task CreateUserAsync_Should_Not_Send_Email_When_Repository_Fails()
    {
        // Arrange
        var emailService = _fixture.EmailService;
        var userRepository = _fixture.UserRepository;
        
        // Configure repository to throw exception
        userRepository.CreateAsync(Arg.Any<User>()).Returns(Task.FromException<User>(new InvalidOperationException("Database error")));
        
        var newUser = new User { Name = "Jane Doe", Email = "jane@example.com" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(newUser));
        
        // Verify email was NOT sent
        await emailService.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }
}

// API Integration Test Fixture using WebApplicationFactory
public class TestWebAppFixture : ApiIntegrationTestFixture<TestWebApp.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Add any additional services for testing
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();
            
            // Use NSubstitute for email service
            var emailService = Substitute.For<IEmailService>();
            services.AddScoped<IEmailService>(_ => emailService);
        });
    }
}

// API Integration Tests using specific base class
public class TestWebAppApiTests : ApiTestBase
{
    public TestWebAppApiTests(TestWebAppFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Get_Test_Endpoint_Should_Return_Success()
    {
        // Arrange
        var endpoint = "/api/test";

        // Act
        var response = await GetAsync(endpoint);

        // Assert
        AssertSuccess(response);
        
        var result = await DeserializeResponseAsync<TestResponse>(response);
        Assert.NotNull(result);
        Assert.NotNull(result.Message);
    }

    [Fact]
    public async Task Get_Test_Endpoint_With_Id_Should_Return_User()
    {
        // Arrange
        var userId = 1;
        var endpoint = $"/api/test/{userId}";

        // Act
        var response = await GetAsync(endpoint);

        // Assert
        AssertStatusCode(response, HttpStatusCode.OK);
        
        var result = await DeserializeResponseAsync<TestResponse>(response);
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
    }

    [Fact]
    public async Task Post_Test_Endpoint_Should_Create_Resource()
    {
        // Arrange
        var endpoint = "/api/test";
        var testData = new { name = "Test Item", value = 123 };
        var content = SerializeToJsonContent(testData);

        // Act
        var response = await PostAsync(endpoint, content);

        // Assert
        AssertStatusCode(response, HttpStatusCode.Created);
    }

    [Fact]
    public async Task Get_Test_Endpoint_With_Custom_Headers_Should_Work()
    {
        // Arrange
        var endpoint = "/api/test";
        var headers = new Dictionary<string, string>
        {
            ["X-Test-Header"] = "test-value",
            ["Authorization"] = "Bearer test-token"
        };

        // Act
        var response = await GetAsync(endpoint, headers);

        // Assert
        AssertSuccess(response);
    }

    // Helper class for test responses
    public class TestResponse
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

// Simple test class to demonstrate the framework works
public class FrameworkTests
{
    [Fact]
    public void Framework_Should_Be_Properly_Configured()
    {
        // This test verifies that the framework is properly set up
        Assert.True(true);
    }
}

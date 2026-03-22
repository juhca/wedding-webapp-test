using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.API.Tests.Helpers;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Application.DTO.Rsvp;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Controllers;

[Trait("Category", "Rsvp System Integration Tests")]
[Collection("Sequential")]
public class RsvpControllerTests(WeddingAppWebApplicationFactory factory) : IClassFixture<WeddingAppWebApplicationFactory>
{
    private readonly HttpClient _client =  factory.CreateClient();

    [Fact]
    public async Task GetMyRsvp_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        await factory.ResetDatabaseAsync();
        // Act
        var response = await _client.GetAsync("/api/rsvp/my");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRsvp_WithValidData_ReturnsOk()
    {
        await factory.ResetDatabaseAsync();
        // Arrange
        var accessCode = "LimitedExperienceAccessCode";

        // Seed the database with a test admin user
        await SeedDatabase(db =>
        {
            var guestUser = TestDataBuilder.CreateGuestUser(accessCode, UserRole.LimitedExperience);
            db.Users.Add(guestUser);
        });
        
        var loginRequest = new GuestLoginRequest(accessCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);
        
        var createDto = new CreateRsvpDto
        {
            IsAttending = true,
            DietaryRestrictions = "Vegetarian",
            Notes = "Excited!",
            Companions =
            [
                new CreateGuestCompanionDto
                {
                    FirstName = "Alice",
                    LastName = "Smith",
                    Age = 28,
                    DietaryRestrictions = "Vegan"
                }
            ]
        };

        // Act
        // Add the token to the Authorization header
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);
        var response = await _client.PostAsJsonAsync("/api/rsvp", createDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var rsvpResponse = await response.Content.ReadFromJsonAsync<RsvpDto>();

        Assert.NotNull(rsvpResponse);
        Assert.Equal(createDto.IsAttending, rsvpResponse.IsAttending);
        Assert.Equal(createDto.DietaryRestrictions, rsvpResponse.DietaryRestrictions);
        Assert.Equal(createDto.Notes, rsvpResponse.Notes);
        Assert.Equal(createDto.Companions.Count, rsvpResponse.Companions.Count);
        Assert.Equal(createDto.Companions[0].FirstName, rsvpResponse.Companions[0].FirstName);
    }

    [Fact]
    public async Task CreateRsvp_ExceedingMaxCompanions_ReturnsBadRequest()
    {
        await factory.ResetDatabaseAsync();
        // Arrange
        var accessCode = "LimitedExperienceAccessCode";

        // Seed the database with a test admin user
        await SeedDatabase(db =>
        {
            var guestUser = TestDataBuilder.CreateGuestUser(accessCode, UserRole.LimitedExperience);
            db.Users.Add(guestUser);
        });
        
        var loginRequest = new GuestLoginRequest(accessCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);
        
        var createDto = new CreateRsvpDto
        {
            IsAttending = true,
            DietaryRestrictions = "Vegetarian",
            Notes = "Excited!",
            Companions =
            [
                new CreateGuestCompanionDto
                {
                    FirstName = "A",
                    LastName = "AA"
                },
                new CreateGuestCompanionDto
                {
                    FirstName = "B",
                    LastName = "BB"
                },
                new CreateGuestCompanionDto
                {
                    FirstName = "C",
                    LastName = "CC"
                }
            ]
        };
        
        // Act
        // Add the token to the Authorization header
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);
        var response = await _client.PostAsJsonAsync("/api/rsvp", createDto);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_AsAdmin_ReturnsStatistics()
    {
        await factory.ResetDatabaseAsync();
        // Arrange
        var email = "admin@wedding.com";
        var password = "SecurePassword123";
        
        var guestAccessCode1 = "GUEST001";
        var guestAccessCode2 = "GUEST002";
        var guestAccessCode3 = "GUEST003";

        // Seed the database with admin and guests with RSVPs
        await SeedDatabase(db =>
        {
            // Add admin user
            var admin = TestDataBuilder.CreateAdminUser(email, password);
            db.Users.Add(admin);
            
            // Add guests
            var guest1 = TestDataBuilder.CreateGuestUser(guestAccessCode1, UserRole.FullExperience);
            var guest2 = TestDataBuilder.CreateGuestUser(guestAccessCode2, UserRole.LimitedExperience);
            var guest3 = TestDataBuilder.CreateGuestUser(guestAccessCode3, UserRole.FullExperience);
            
            db.Users.AddRange(guest1, guest2, guest3);
            
            // Add RSVPs
            var rsvp1 = new Rsvp
            {
                Id = Guid.NewGuid(),
                UserId = guest1.Id,
                IsAttending = true,
                RespondedAt = DateTime.UtcNow,
                DietaryRestrictions = "Vegetarian",
                CreatedAt = DateTime.UtcNow,
                Companions = new List<GuestCompanion>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Alice",
                        LastName = "Smith",
                        Age = 28,
                        DietaryRestrictions = "Vegan",
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };
            
            var rsvp2 = new Rsvp
            {
                Id = Guid.NewGuid(),
                UserId = guest2.Id,
                IsAttending = false,
                RespondedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Companions = new List<GuestCompanion>()
            };
            
            // guest3 has no RSVP yet (pending)
            
            db.Rsvps.AddRange(rsvp1, rsvp2);
        });

        // Login as admin
        var loginRequest = new AdminLoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/AdminLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);
        
        // Act
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);
        var response = await _client.GetAsync("/api/rsvp/summary");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var summary = await response.Content.ReadFromJsonAsync<RsvpSummaryDto>();
        
        Assert.NotNull(summary);
        Assert.Equal(3, summary.TotalInvited); // 3 guests total
        Assert.Equal(2, summary.TotalResponded); // 2 RSVPs submitted
        Assert.Equal(1, summary.TotalAttending); // 1 attending
        Assert.Equal(1, summary.TotalNotAttending); // 1 not attending
        Assert.Equal(2, summary.TotalPeople); // 1 main guest + 1 companion
        Assert.Equal(1, summary.TotalCompanions); // 1 companion total
        Assert.Equal(1, summary.PendingResponses); // 1 guest hasn't responded
        Assert.Single(summary.AttendingGuests);
        Assert.Single(summary.NotAttendingGuests);
        Assert.Single(summary.PendingGuests);
    }

    [Fact]
    public async Task ExportCatering_AsAdmin_ReturnsCsvFile()
    {
        await factory.ResetDatabaseAsync();
        // Arrange
        var email = "admin@wedding.com";
        var password = "SecurePassword123";
        
        var guestAccessCode1 = "GUEST001";
        var guestAccessCode2 = "GUEST002";

        // Seed the database with admin and attending guests
        await SeedDatabase(db =>
        {
            // Add admin user
            var admin = TestDataBuilder.CreateAdminUser(email, password);
            db.Users.Add(admin);
            
            // Add guests
            var guest1 = TestDataBuilder.CreateGuestUser(guestAccessCode1, UserRole.FullExperience);
            guest1.FirstName = "John";
            guest1.LastName = "Doe";
            guest1.Email = "john@example.com";
            
            var guest2 = TestDataBuilder.CreateGuestUser(guestAccessCode2, UserRole.LimitedExperience);
            guest2.FirstName = "Jane";
            guest2.LastName = "Smith";
            guest2.Email = "jane@example.com";
            
            db.Users.AddRange(guest1, guest2);
            
            // Add RSVPs with companions
            var rsvp1 = new Rsvp
            {
                Id = Guid.NewGuid(),
                UserId = guest1.Id,
                IsAttending = true,
                RespondedAt = DateTime.UtcNow,
                DietaryRestrictions = "Vegetarian",
                Notes = "Looking forward!",
                CreatedAt = DateTime.UtcNow,
                Companions = new List<GuestCompanion>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Alice",
                        LastName = "Doe",
                        Age = 28,
                        DietaryRestrictions = "Vegan",
                        Notes = "Sister",
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Bob",
                        LastName = "Doe",
                        Age = 5,
                        DietaryRestrictions = "No nuts",
                        Notes = "Nephew",
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };
            
            var rsvp2 = new Rsvp
            {
                Id = Guid.NewGuid(),
                UserId = guest2.Id,
                IsAttending = true,
                RespondedAt = DateTime.UtcNow,
                DietaryRestrictions = "Gluten-free",
                CreatedAt = DateTime.UtcNow,
                Companions = new List<GuestCompanion>()
            };
            
            db.Rsvps.AddRange(rsvp1, rsvp2);
        });

        // Login as admin
        var loginRequest = new AdminLoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/AdminLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);
        
        // Act
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);
        var response = await _client.GetAsync("/api/rsvp/export/catering");
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        // Check content type
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        
        // Check filename contains date
        var contentDisposition = response.Content.Headers.ContentDisposition;
        Assert.NotNull(contentDisposition);
        Assert.Contains("catering-export-", contentDisposition.FileName);
        Assert.Contains(".csv", contentDisposition.FileName);
        
        // Read and verify CSV content
        var csv = await response.Content.ReadAsStringAsync();
        
        // Verify CSV header
        Assert.Contains("GuestType,FirstName,LastName,Age,DietaryRestrictions,Notes,MainGuestEmail", csv);
        
        // Verify main guests are present
        Assert.Contains("\"Main\",\"John\",\"Doe\"", csv);
        Assert.Contains("\"Main\",\"Jane\",\"Smith\"", csv);
        
        // Verify companions are present
        Assert.Contains("\"Companion\",\"Alice\",\"Doe\",28,\"Vegan\"", csv);
        Assert.Contains("\"Companion\",\"Bob\",\"Doe\",5,\"No nuts\"", csv);
        
        // Verify emails are present
        Assert.Contains("john@example.com", csv);
        Assert.Contains("jane@example.com", csv);
        
        // Count lines (header + 2 main guests + 2 companions = 5 lines)
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(5, lines.Length);
    }
    
    #region HelperMethods
    /// <summary>
    /// Helper method to seed the database with test data.
    /// Creates a new scope and disposes it properly after seeding.
    /// </summary>
    private async Task SeedDatabase(Action<AppDbContext> seedAction)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        seedAction(db);
        await db.SaveChangesAsync();
    }
    #endregion
}
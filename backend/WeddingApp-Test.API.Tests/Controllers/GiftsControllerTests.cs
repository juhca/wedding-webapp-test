using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.API.Tests.Helpers;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Gift;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Controllers;

[Trait("Category", "GiftsController Integration Tests")]
public class GiftsControllerTests(WeddingAppWebApplicationFactory factory)
    : IClassFixture<WeddingAppWebApplicationFactory>
{
    private readonly HttpClient _client =  factory.CreateClient();
    
    
    #region GET TESTS
    [Fact]
    public async Task GetAllGifts_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        await factory.ResetDatabaseAsync();
        
        // Act
        var response = await _client.GetAsync("/api/gifts");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAllGifts_AsAuthenticatedUser_ReturnsVisibleGifts()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var accessCode = "GUEST001";
        Guid visibleGiftId = Guid.Empty;
        Guid hiddenGiftId = Guid.Empty;

        await SeedDatabase(db =>
        {
            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            db.Users.Add(guest);

            var visibleGift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Visible Gift",
                Description = "This gift is visible",
                Price = 100.00m,
                MaxReservations = 1,
                IsVisible = true,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            };
            visibleGiftId = visibleGift.Id;

            var hiddenGift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Hidden Gift",
                Description = "This gift is hidden",
                Price = 200.00m,
                MaxReservations = 1,
                IsVisible = false,
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            };
            hiddenGiftId = hiddenGift.Id;

            db.Gifts.AddRange(visibleGift, hiddenGift);
        });

        var loginRequest = new GuestLoginRequest(accessCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.GetAsync("/api/gifts");

        // Assert
        response.EnsureSuccessStatusCode();
        var gifts = await response.Content.ReadFromJsonAsync<List<GiftDto>>();

        Assert.NotNull(gifts);
        Assert.Single(gifts); // Only visible gift
        Assert.Equal("Visible Gift", gifts[0].Name);
        Assert.Equal(visibleGiftId, gifts[0].Id);
        Assert.DoesNotContain(gifts, g => g.Id == hiddenGiftId);
    }
    
    [Fact]
    public async Task GetGiftById_ExistingGift_ReturnsGift()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var accessCode = "GUEST001";
        Guid giftId = Guid.Empty;

        await SeedDatabase(db =>
        {
            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            db.Users.Add(guest);

            var gift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Test Gift",
                Description = "A wonderful gift",
                Price = 150.00m,
                MaxReservations = 1,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };
            giftId = gift.Id;
            db.Gifts.Add(gift);
        });

        var loginRequest = new GuestLoginRequest(accessCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.GetAsync($"/api/gifts/{giftId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var gift = await response.Content.ReadFromJsonAsync<GiftDto>();

        Assert.NotNull(gift);
        Assert.Equal("Test Gift", gift.Name);
        Assert.Equal(150.00m, gift.Price);
    }
    
    [Fact]
    public async Task GetGiftById_NonExistentGift_ReturnsNotFound()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var accessCode = "GUEST001";
        await SeedDatabase(db =>
        {
            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            db.Users.Add(guest);
        });

        var loginRequest = new GuestLoginRequest(accessCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.GetAsync($"/api/gifts/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetMyReservations_ReturnsOnlyMyReservedGifts()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var accessCode1 = "GUEST001";
        var accessCode2 = "GUEST002";
        Guid gift1Id = Guid.Empty;
        Guid gift2Id = Guid.Empty;

        await SeedDatabase(db =>
        {
            var guest1 = TestDataBuilder.CreateGuestUser(accessCode1, UserRole.FullExperience);
            var guest2 = TestDataBuilder.CreateGuestUser(accessCode2, UserRole.FullExperience);
            db.Users.AddRange(guest1, guest2);

            var gift1 = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Gift 1",
                MaxReservations = 2,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };
            gift1Id = gift1.Id;

            var gift2 = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Gift 2",
                MaxReservations = 1,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };
            gift2Id = gift2.Id;

            db.Gifts.AddRange(gift1, gift2);

            // Guest 1 reserves Gift 1
            db.GiftReservations.Add(new GiftReservation
            {
                Id = Guid.NewGuid(),
                GiftId = gift1.Id,
                ReservedByUserId = guest1.Id,
                ReservedAt = DateTime.UtcNow
            });

            // Guest 2 reserves Gift 2
            db.GiftReservations.Add(new GiftReservation
            {
                Id = Guid.NewGuid(),
                GiftId = gift2.Id,
                ReservedByUserId = guest2.Id,
                ReservedAt = DateTime.UtcNow
            });
        });

        var loginRequest = new GuestLoginRequest(accessCode1);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.GetAsync("/api/gifts/my-reservations");

        // Assert
        response.EnsureSuccessStatusCode();
        var gifts = await response.Content.ReadFromJsonAsync<List<GiftDto>>();

        Assert.NotNull(gifts);
        Assert.Single(gifts); // Only Gift 1
        Assert.Equal("Gift 1", gifts[0].Name);
        Assert.Equal(gift1Id, gifts[0].Id);
        Assert.DoesNotContain(gifts, g => g.Id == gift2Id);
        Assert.True(gifts[0].IsReservedByMe);
    }
    #endregion
    
    #region RESERVATION TESTS
    [Fact]
    public async Task ReserveGift_SingleUse_Success()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var accessCode = "GUEST001";
        Guid giftId = Guid.Empty;

        await SeedDatabase(db =>
        {
            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            db.Users.Add(guest);

            var gift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Cutlery Set",
                Description = "Beautiful cutlery",
                Price = 149.99m,
                MaxReservations = 1, // Single-use
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };
            giftId = gift.Id;
            db.Gifts.Add(gift);
        });

        var loginRequest = new GuestLoginRequest(accessCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        var reserveDto = new ReserveGiftDto
        {
            WantsReminder = true,
            ReminderDate = DateTime.UtcNow.AddDays(30),
            Notes = "Can't wait to use it!"
        };

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.PostAsJsonAsync($"/api/gifts/{giftId}/reserve", reserveDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GiftReservationConfirmationDto>();

        Assert.NotNull(result);
        Assert.Equal("Cutlery Set", result.GiftName);
        Assert.Equal(0, result.RemainingReservations);
        Assert.True(result.GiftFullyReserved);
        Assert.True(result.ReminderScheduled);
        Assert.Contains("reserved successfully", result.Message);
    }
    
    [Fact]
    public async Task ReserveGift_MultiUse_Success()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var accessCode1 = "GUEST001";
        var accessCode2 = "GUEST002";
        Guid giftId = Guid.Empty;

        await SeedDatabase(db =>
        {
            var guest1 = TestDataBuilder.CreateGuestUser(accessCode1, UserRole.FullExperience);
            var guest2 = TestDataBuilder.CreateGuestUser(accessCode2, UserRole.FullExperience);
            db.Users.AddRange(guest1, guest2);

            var gift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "$50 Gift Card",
                Description = "Amazon gift card",
                Price = 50.00m,
                MaxReservations = 10, // Multi-use
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };
            giftId = gift.Id;
            db.Gifts.Add(gift);
        });

        // First reservation
        var loginRequest1 = new GuestLoginRequest(accessCode1);
        var loginResponse1 = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest1);
        var loginResult1 = await loginResponse1.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse1.EnsureSuccessStatusCode();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult1!.Token);
        var response1 = await _client.PostAsJsonAsync($"/api/gifts/{giftId}/reserve", new ReserveGiftDto());

        // Assert first reservation
        response1.EnsureSuccessStatusCode();
        var result1 = await response1.Content.ReadFromJsonAsync<GiftReservationConfirmationDto>();
        Assert.NotNull(result1);
        Assert.Equal(9, result1.RemainingReservations);
        Assert.False(result1.GiftFullyReserved);

        // Second reservation by different user
        var loginRequest2 = new GuestLoginRequest(accessCode2);
        var loginResponse2 = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest2);
        var loginResult2 = await loginResponse2.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse2.EnsureSuccessStatusCode();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult2!.Token);
        var response2 = await _client.PostAsJsonAsync($"/api/gifts/{giftId}/reserve", new ReserveGiftDto());

        // Assert second reservation
        response2.EnsureSuccessStatusCode();
        var result2 = await response2.Content.ReadFromJsonAsync<GiftReservationConfirmationDto>();
        Assert.NotNull(result2);
        Assert.Equal(8, result2.RemainingReservations);
        Assert.False(result2.GiftFullyReserved);
    }
    
    [Fact]
    public async Task ReserveGift_AlreadyFullyReserved_ReturnsBadRequest()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var accessCode1 = "GUEST001";
        var accessCode2 = "GUEST002";
        Guid giftId = Guid.Empty;

        await SeedDatabase(db =>
        {
            var guest1 = TestDataBuilder.CreateGuestUser(accessCode1, UserRole.FullExperience);
            var guest2 = TestDataBuilder.CreateGuestUser(accessCode2, UserRole.FullExperience);
            db.Users.AddRange(guest1, guest2);

            var gift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Limited Gift",
                MaxReservations = 1,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };
            giftId = gift.Id;
            db.Gifts.Add(gift);

            // Already reserved by guest1
            db.GiftReservations.Add(new GiftReservation
            {
                Id = Guid.NewGuid(),
                GiftId = gift.Id,
                ReservedByUserId = guest1.Id,
                ReservedAt = DateTime.UtcNow
            });
        });

        var loginRequest = new GuestLoginRequest(accessCode2);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        // Act - Guest 2 tries to reserve fully reserved gift
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.PostAsJsonAsync($"/api/gifts/{giftId}/reserve", new ReserveGiftDto());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("fully reserved", errorContent);
    }
    
    [Fact]
    public async Task ReserveGift_UserAlreadyReserved_ReturnsBadRequest()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var accessCode = "GUEST001";
        Guid giftId = Guid.Empty;

        await SeedDatabase(db =>
        {
            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            db.Users.Add(guest);

            var gift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Multi Gift",
                MaxReservations = 5,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };
            giftId = gift.Id;
            db.Gifts.Add(gift);

            // Already reserved by this user
            db.GiftReservations.Add(new GiftReservation
            {
                Id = Guid.NewGuid(),
                GiftId = gift.Id,
                ReservedByUserId = guest.Id,
                ReservedAt = DateTime.UtcNow
            });
        });

        var loginRequest = new GuestLoginRequest(accessCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        // Act - Try to reserve again
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.PostAsJsonAsync($"/api/gifts/{giftId}/reserve", new ReserveGiftDto());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("already reserved", errorContent);
    }
    
    [Fact]
    public async Task ReserveGift_NonExistentGift_ReturnsNotFound()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var accessCode = "GUEST001";
        await SeedDatabase(db =>
        {
            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            db.Users.Add(guest);
        });

        var loginRequest = new GuestLoginRequest(accessCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.PostAsJsonAsync($"/api/gifts/{Guid.NewGuid()}/reserve", new ReserveGiftDto());

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task UnreserveGift_Success()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var accessCode = "GUEST001";
        Guid giftId = Guid.Empty;

        await SeedDatabase(db =>
        {
            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            db.Users.Add(guest);

            var gift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Test Gift",
                MaxReservations = 1,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };
            giftId = gift.Id;
            db.Gifts.Add(gift);

            // Reserved by user
            db.GiftReservations.Add(new GiftReservation
            {
                Id = Guid.NewGuid(),
                GiftId = gift.Id,
                ReservedByUserId = guest.Id,
                ReservedAt = DateTime.UtcNow
            });
        });

        var loginRequest = new GuestLoginRequest(accessCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.DeleteAsync($"/api/gifts/{giftId}/reserve");

        // Assert
        response.EnsureSuccessStatusCode();

        // Verify gift is available again
        var getResponse = await _client.GetAsync($"/api/gifts/{giftId}");
        var gift = await getResponse.Content.ReadFromJsonAsync<GiftDto>();
        Assert.NotNull(gift);
        Assert.Equal(0, gift.ReservationCount);
        Assert.False(gift.IsFullyReserved);
        Assert.False(gift.IsReservedByMe);
    }
    
    [Fact]
    public async Task UnreserveGift_NotReservedByUser_ReturnsNotFound()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var accessCode = "GUEST001";
        Guid giftId = Guid.Empty;

        await SeedDatabase(db =>
        {
            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            db.Users.Add(guest);

            var gift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Test Gift",
                MaxReservations = 1,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };
            giftId = gift.Id;
            db.Gifts.Add(gift);
            // No reservation
        });

        var loginRequest = new GuestLoginRequest(accessCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.DeleteAsync($"/api/gifts/{giftId}/reserve");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    #endregion
    
    #region ADMIN CRUD TESTS
    [Fact]
    public async Task CreateGift_AsAdmin_Success()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var email = "admin@wedding.com";
        var password = "SecurePassword123";

        await SeedDatabase(db =>
        {
            var admin = TestDataBuilder.CreateAdminUser(email, password);
            db.Users.Add(admin);
        });

        var loginRequest = new AdminLoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/AdminLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        var createDto = new CreateGiftDto
        {
            Name = "New Gift",
            Description = "A brand new gift",
            Price = 99.99m,
            MaxReservations = 1,
            DisplayOrder = 1,
            IsVisible = true
        };

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.PostAsJsonAsync("/api/gifts", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var gift = await response.Content.ReadFromJsonAsync<GiftDto>();

        Assert.NotNull(gift);
        Assert.Equal("New Gift", gift.Name);
        Assert.Equal(99.99m, gift.Price);
        Assert.Equal(1, gift.MaxReservations);
    }
    
    [Fact]
    public async Task CreateGift_AsNonAdmin_ReturnsForbidden()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var accessCode = "GUEST001";
        await SeedDatabase(db =>
        {
            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            db.Users.Add(guest);
        });

        var loginRequest = new GuestLoginRequest(accessCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        var createDto = new CreateGiftDto
        {
            Name = "Unauthorized Gift",
            MaxReservations = 1
        };

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.PostAsJsonAsync("/api/gifts", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateGift_AsAdmin_Success()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var email = "admin@wedding.com";
        var password = "SecurePassword123";
        Guid giftId = Guid.Empty;

        await SeedDatabase(db =>
        {
            var admin = TestDataBuilder.CreateAdminUser(email, password);
            db.Users.Add(admin);

            var gift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Old Name",
                Price = 100.00m,
                MaxReservations = 1,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };
            giftId = gift.Id;
            db.Gifts.Add(gift);
        });

        var loginRequest = new AdminLoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/AdminLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        var updateDto = new UpdateGiftDto
        {
            Name = "Updated Name",
            Description = "Updated description",
            Price = 150.00m,
            MaxReservations = 2,
            DisplayOrder = 1,
            IsVisible = true
        };

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.PutAsJsonAsync($"/api/gifts/{giftId}", updateDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var gift = await response.Content.ReadFromJsonAsync<GiftDto>();

        Assert.NotNull(gift);
        Assert.Equal("Updated Name", gift.Name);
        Assert.Equal(150.00m, gift.Price);
        Assert.Equal(2, gift.MaxReservations);
    }
    
    [Fact]
    public async Task DeleteGift_AsAdmin_WithNoReservations_Success()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var email = "admin@wedding.com";
        var password = "SecurePassword123";
        Guid giftId = Guid.Empty;

        await SeedDatabase(db =>
        {
            var admin = TestDataBuilder.CreateAdminUser(email, password);
            db.Users.Add(admin);

            var gift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Gift to Delete",
                MaxReservations = 1,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };
            giftId = gift.Id;
            db.Gifts.Add(gift);
        });

        var loginRequest = new AdminLoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/AdminLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.DeleteAsync($"/api/gifts/{giftId}");

        // Assert
        response.EnsureSuccessStatusCode();

        // Verify gift is deleted
        var getResponse = await _client.GetAsync($"/api/gifts/{giftId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
    
    [Fact]
    public async Task DeleteGift_AsAdmin_WithReservations_ReturnsBadRequest()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var email = "admin@wedding.com";
        var password = "SecurePassword123";
        var guestAccessCode = "GUEST001";
        Guid giftId = Guid.Empty;

        await SeedDatabase(db =>
        {
            var admin = TestDataBuilder.CreateAdminUser(email, password);
            var guest = TestDataBuilder.CreateGuestUser(guestAccessCode, UserRole.FullExperience);
            db.Users.AddRange(admin, guest);

            var gift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Reserved Gift",
                MaxReservations = 1,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };
            giftId = gift.Id;
            db.Gifts.Add(gift);

            // Add reservation
            db.GiftReservations.Add(new GiftReservation
            {
                Id = Guid.NewGuid(),
                GiftId = gift.Id,
                ReservedByUserId = guest.Id,
                ReservedAt = DateTime.UtcNow
            });
        });

        var loginRequest = new AdminLoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/AdminLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.DeleteAsync($"/api/gifts/{giftId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("has reservations", errorContent);
    }
    #endregion
    
    #region GIFT TYPES TESTS
    [Fact]
    public async Task ReservationStatus_UnlimitedGift_ShowsCorrectStatus()
    {
        await factory.ResetDatabaseAsync();
        
        // Arrange
        var accessCode1 = "GUEST001";
        var accessCode2 = "GUEST002";
        Guid giftId = Guid.Empty;

        await SeedDatabase(db =>
        {
            var guest1 = TestDataBuilder.CreateGuestUser(accessCode1, UserRole.FullExperience);
            var guest2 = TestDataBuilder.CreateGuestUser(accessCode2, UserRole.FullExperience);
            db.Users.AddRange(guest1, guest2);

            var gift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Honeymoon Fund",
                MaxReservations = null, // Unlimited
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };
            giftId = gift.Id;
            db.Gifts.Add(gift);

            // Add 2 reservations
            db.GiftReservations.Add(new GiftReservation
            {
                Id = Guid.NewGuid(),
                GiftId = gift.Id,
                ReservedByUserId = guest1.Id,
                ReservedAt = DateTime.UtcNow
            });

            db.GiftReservations.Add(new GiftReservation
            {
                Id = Guid.NewGuid(),
                GiftId = gift.Id,
                ReservedByUserId = guest2.Id,
                ReservedAt = DateTime.UtcNow
            });
        });

        var loginRequest = new GuestLoginRequest(accessCode1);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();

        // Act
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.GetAsync($"/api/gifts/{giftId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var gift = await response.Content.ReadFromJsonAsync<GiftDto>();

        Assert.NotNull(gift);
        Assert.Null(gift.MaxReservations);
        Assert.Equal(2, gift.ReservationCount);
        Assert.Null(gift.RemainingReservations);
        Assert.False(gift.IsFullyReserved);
        Assert.Contains("unlimited", gift.ReservationStatus.ToLower());
    }
    #endregion
    
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
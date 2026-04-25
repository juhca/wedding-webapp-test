using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.Infrastructure.Data;

public static class DataSeeder
{
    /// <summary>
    /// Seed initial development data
    /// </summary>
    public static void SeedDevelopmentData(AppDbContext context, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        // Ensure database exists
        context.Database.EnsureCreated();

        // Seed Admin User
        var admin = SeedAdminUser(context, passwordHasher);

        // Seed Wedding Info
        SeedWeddingInfo(context);

        // Print dev token
        PrintDevToken(admin, tokenService);
        
        // Seed Sample Guests
        SeedSampleGuests(context);
        
        // Seed Email templates
        SeedEmailTemplatesAsync(context);
    }
    
    /// <summary>
    /// Seed admin user for development
    /// </summary>
    private static User SeedAdminUser(AppDbContext context, IPasswordHasher passwordHasher)
    {
        var admin = context.Users.FirstOrDefault(u => u.Role == UserRole.Admin);

        if (admin is null)
        {
            passwordHasher.CreatePasswordHash("Admin123!", out var hash, out var salt);

            admin = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Dev",
                LastName = "Admin",
                Email = "admin@dev.local",
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = UserRole.Admin
            };

            context.Users.Add(admin);
            context.SaveChanges();

            Console.WriteLine("Admin user created");
            Console.WriteLine($"  Email: {admin.Email}");
            Console.WriteLine($"  Password: Admin123!");
        }
        else
        {
            Console.WriteLine("  Admin user already exists");
        }

        return admin;
    }
    
    /// <summary>
    /// Seed wedding info
    /// </summary>
    private static void SeedWeddingInfo(AppDbContext context)
    {
        var weddingInfo = context.WeddingInfo.FirstOrDefault();

        if (weddingInfo is null)
        {
            weddingInfo = new WeddingInfo
            {
                Id = Guid.NewGuid(),

                // Public info
                BrideName = "Jane",
                BrideSurname = "Doe",
                GroomName = "John",
                GroomSurname = "Toe",
                ApproximateDate = "Summer 2027",
                WeddingName = "Jane & John's Wedding Day",
                WeddingDescription = "Join us as we celebrate our special day in beautiful Ljubljana!",

                // Authenticated info
                WeddingDate = new DateTime(2027, 6, 19, 12, 45, 0),

                // Civil ceremony location
                CivilLocationName = "Ljubljana City Hall",
                CivilLocationAddress = "Mestni trg 1, 1000 Ljubljana, Slovenia",
                CivilLocationLatitude = 46.0503,
                CivilLocationLongitude = 14.5069,
                CivilLocationGoogleMapsUrl = "https://maps.google.com/?q=46.0503,14.5069",
                CivilLocationAppleMapsUrl = "https://maps.apple/p/TJGBEw8woLWQkY",

                // Church ceremony location
                ChurchLocationName = "St. Nicholas Cathedral",
                ChurchLocationAddress = "Dolničarjeva ulica 1, 1000 Ljubljana, Slovenia",
                ChurchLocationLatitude = 46.0512,
                ChurchLocationLongitude = 14.5082,
                ChurchLocationGoogleMapsUrl = "https://maps.google.com/?q=46.0512,14.5082",
                ChurchLocationAppleMapsUrl = "https://maps.apple/p/AHaroy3p-cuLhm",

                // Party location (Full + Admin)
                PartyLocationName = "Grand Hotel Union",
                PartyLocationAddress = "Miklošičeva cesta 1, 1000 Ljubljana, Slovenia",
                PartyLocationLatitude = 46.0546,
                PartyLocationLongitude = 14.5066,
                PartyLocationGoogleMapsUrl = "https://maps.google.com/?q=46.0546,14.5066",
                PartyLocationAppleMapsUrl = "https://maps.apple/p/Hojxx.d~SoEmPg",

                // Livestream
                LivestreamUrl = "https://youtube.com/live/example123",

                // House location (Admin only)
                HouseLocationName = "Bride's Family Home",
                HouseLocationAddress = "Trubarjeva cesta 50, 1000 Ljubljana, Slovenia",
                HouseLocationLatitude = 46.0522,
                HouseLocationLongitude = 14.5155,
                HouseLocationGoogleMapsUrl = "https://maps.google.com/?q=46.0522,14.5155",
                HouseLocationAppleMapsUrl = "https://maps.apple/p/hd4kZL9qdjva7i",

                CreatedAt = DateTime.UtcNow
            };

            context.WeddingInfo.Add(weddingInfo);
            context.SaveChanges();

            Console.WriteLine("Wedding info seeded");
            Console.WriteLine($"  Couple: {weddingInfo.BrideName} & {weddingInfo.GroomName}");
            Console.WriteLine($"  Date: {weddingInfo.WeddingDate:dd.MM.yyyy}");
        }
        else
        {
            Console.WriteLine("  Wedding info already exists");
        }
    }
    
    /// <summary>
    /// Print dev token for easy testing
    /// </summary>
    private static void PrintDevToken(User admin, ITokenService tokenService)
    {
        var devToken = tokenService.CreateJwtToken(admin);

        Console.WriteLine("");
        Console.WriteLine("========================================");
        Console.WriteLine("   DEV ADMIN TOKEN (copy to Swagger)");
        Console.WriteLine("========================================");
        Console.WriteLine($" Bearer {devToken}");
        Console.WriteLine("========================================");
        Console.WriteLine("");
    }
    
    /// <summary>
    /// Seed sample guests for testing (optional)
    /// </summary>
    private static void SeedSampleGuests(AppDbContext context)
    {
        if (context.Users.Count() > 1)
        {
            Console.WriteLine("  Sample guests already exist");
            return;
        }

        var sampleGuests = new[]
        {
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                AccessCode = "LITE1234",
                Role = UserRole.LimitedExperience
            },
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob@example.com",
                AccessCode = "FULL5678",
                Role = UserRole.FullExperience
            },
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Charlie",
                LastName = "Brown",
                Email = "charlie@example.com",
                AccessCode = "LITE9012",
                Role = UserRole.LimitedExperience
            }
        };

        context.Users.AddRange(sampleGuests);
        context.SaveChanges();

        Console.WriteLine("  Sample guests created:");
        foreach (var guest in sampleGuests)
        {
            Console.WriteLine($"   {guest.FirstName} {guest.LastName} ({guest.Role}) - Code: {guest.AccessCode}");
        }
    }
    
    private static void SeedEmailTemplatesAsync(AppDbContext context)
    {
        if (context.EmailTemplates.Any())
        {
            return; // already seeded
        }

        var templates = new List<EmailTemplate>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "RSVP Confirmation",
                TriggerName = "rsvp.submitted",
                SubjectTemplate = "Thanks for your RSVP, {{ User.FirstName }}!",
                HtmlBodyTemplate = """
                    <h1>Thank you, {{ User.FirstName }}!</h1>
                    {% if Rsvp.IsAttending %}
                    <p>We're thrilled you'll be joining us at <strong>{{ Wedding.WeddingName }}</strong>.</p>
                    {% else %}
                    <p>We're sorry you can't make it, but thanks for letting us know.</p>
                    {% endif %}
                    """,
                PlainTextBodyTemplate = "Thanks for your RSVP, {{ User.FirstName }}! {% if Rsvp.IsAttending %}We'll see you there.{% else %}Sorry you can't make it.{% endif %}",
                IsActive = true,
                MaxSendsPerUser = 1,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "RSVP Updated",
                TriggerName = "rsvp.updated",
                SubjectTemplate = "Your RSVP has been updated, {{ User.FirstName }}",
                HtmlBodyTemplate = """
                    <h1>RSVP Updated</h1>
                    <p>Hi {{ User.FirstName }}, your RSVP for <strong>{{ Wedding.WeddingName }}</strong> has been updated.</p>
                    {% if Rsvp.IsAttending %}
                    <p>Status: <strong>Attending</strong></p>
                    {% else %}
                    <p>Status: <strong>Not attending</strong></p>
                    {% endif %}
                    """,
                PlainTextBodyTemplate = null,
                IsActive = true,
                MaxSendsPerUser = null,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Gift Reserved",
                TriggerName = "gift.reserved",
                SubjectTemplate = "Gift reserved: {{ Gift.Name }}",
                HtmlBodyTemplate = """
                    <h1>Thank you, {{ User.FirstName }}!</h1>
                    <p>You've reserved <strong>{{ Gift.Name }}</strong>.</p>
                    {% if Gift.PurchaseLink %}
                    <p><a href="{{ Gift.PurchaseLink }}">Purchase here</a></p>
                    {% endif %}
                    """,
                PlainTextBodyTemplate = null,
                IsActive = true,
                MaxSendsPerUser = null,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Gift Unreserved",
                TriggerName = "gift.unreserved",
                SubjectTemplate = "Reservation cancelled: {{ Gift.Name }}",
                HtmlBodyTemplate = """
                    <h1>Reservation Cancelled</h1>
                    <p>Hi {{ User.FirstName }}, your reservation for <strong>{{ Gift.Name }}</strong> has been cancelled.</p>
                    """,
                PlainTextBodyTemplate = null,
                IsActive = true,
                MaxSendsPerUser = null,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.EmailTemplates.AddRange(templates);
        context.SaveChanges();
    }
}
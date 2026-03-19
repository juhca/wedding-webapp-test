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
        
        // Seed default email templates
        SeedEmailTemplates(context);
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

    /// <summary>
    /// Seed the 8 default email templates that replace the old hardcoded emails.
    /// Idempotent — only inserts if no templates exist yet.
    /// </summary>
    private static void SeedEmailTemplates(AppDbContext context)
    {
        if (context.EmailTemplates.Any())
        {
            Console.WriteLine("  Email templates already seeded");
            return;
        }

        // Liquid templates use {{ }} and {% %} — these are NOT C# interpolation.
        // We use plain string concatenation to inject the subtitle and body into the shared layout.
        const string LayoutOpen =
            "<!DOCTYPE html><html><body style=\"font-family:sans-serif;background:#f9f9f9;padding:24px\">" +
            "<div style=\"max-width:600px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden\">" +
            "<div style=\"background:#1a1a2e;color:#e8c99a;padding:24px;text-align:center\">" +
            "<h1 style=\"margin:0;font-size:22px\">{{ wedding.name }}</h1>";

        const string LayoutCountdown =
            "</div><div style=\"padding:24px\">";

        const string LayoutFooter =
            "</div>" +
            "{% if countdown.imageUrl %}" +
            "<div style=\"text-align:center;padding:16px;background:#f0f0f0\">" +
            "<img src=\"{{ countdown.imageUrl }}\" alt=\"{{ countdown.days }} days to go\" " +
            "style=\"max-width:400px;border-radius:6px\" />" +
            "<p style=\"font-size:12px;color:#999;margin:8px 0 0\">" +
            "If the image doesn't animate, {{ countdown.days }} days until the wedding." +
            "</p></div>{% endif %}" +
            "<div style=\"background:#f5f5f5;color:#888;font-size:12px;text-align:center;padding:16px\">" +
            "This is an automated message. Please do not reply to this email." +
            "</div></div></body></html>";

        static string Wrap(string subtitle, string body)
            => LayoutOpen
               + "<p style=\"margin:4px 0 0;font-size:14px;color:#ccc\">" + subtitle + "</p>"
               + LayoutCountdown
               + body
               + LayoutFooter;

        var templates = new List<EmailTemplate>
        {
            // ── Transactional ─────────────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Name = "RSVP Confirmation",
                Subject = "Your RSVP has been received",
                HtmlBody = Wrap("RSVP Confirmed",
                    "<p>Hi {{ user.firstName }},</p>" +
                    "<p>We've received your RSVP. Here's a summary:</p>" +
                    "<ul><li><strong>Attending:</strong> {{ rsvp.isAttending }}</li>" +
                    "<li><strong>Companions:</strong> {{ rsvp.companionCount }}</li></ul>" +
                    "<p>See you on <strong>{{ wedding.date }}</strong>!</p>"),
                TriggerType = TriggerType.OnEvent,
                EventName = "RsvpSubmitted",
                AudienceType = AudienceType.TriggeredUser,
                MaxSendsPerUser = 1,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "RSVP Updated",
                Subject = "Your RSVP has been updated",
                HtmlBody = Wrap("RSVP Updated",
                    "<p>Hi {{ user.firstName }},</p>" +
                    "<p>Your RSVP has been updated:</p>" +
                    "<ul><li><strong>Attending:</strong> {{ rsvp.isAttending }}</li>" +
                    "<li><strong>Companions:</strong> {{ rsvp.companionCount }}</li></ul>"),
                TriggerType = TriggerType.OnEvent,
                EventName = "RsvpUpdated",
                AudienceType = AudienceType.TriggeredUser,
                MaxSendsPerUser = null,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Gift Reserved",
                Subject = "You've reserved a gift!",
                HtmlBody = Wrap("Gift Reserved",
                    "<p>Hi {{ user.firstName }},</p>" +
                    "<p>You've reserved <strong>{{ gift.name }}</strong> from the gift registry.</p>" +
                    "{% if gift.purchaseLink %}" +
                    "<p><a href=\"{{ gift.purchaseLink }}\" style=\"color:#1a1a2e\">Buy it here</a></p>" +
                    "{% endif %}" +
                    "<p>Thank you so much — this means the world to us!</p>"),
                TriggerType = TriggerType.OnEvent,
                EventName = "GiftReserved",
                AudienceType = AudienceType.TriggeredUser,
                MaxSendsPerUser = null,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Gift Unreserved",
                Subject = "Your gift reservation has been cancelled",
                HtmlBody = Wrap("Gift Unreserved",
                    "<p>Hi {{ user.firstName }},</p>" +
                    "<p>Your reservation for <strong>{{ gift.name }}</strong> has been cancelled.</p>" +
                    "<p>The gift is now available for others to reserve.</p>"),
                TriggerType = TriggerType.OnEvent,
                EventName = "GiftUnreserved",
                AudienceType = AudienceType.TriggeredUser,
                MaxSendsPerUser = null,
                IsActive = true
            },

            // ── Scheduled reminders ───────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Wedding Reminder (7 days)",
                Subject = "Only {{ countdown.days }} days until the big day!",
                HtmlBody = Wrap("Wedding Reminder",
                    "<p>Hi {{ user.firstName }},</p>" +
                    "<p>The wedding is just <strong>{{ countdown.days }} days</strong> away!</p>" +
                    "<p><strong>Date:</strong> {{ wedding.date }}</p>" +
                    "<p><strong>Ceremony:</strong> {{ wedding.ceremonyLocation }}</p>" +
                    "<p>We can't wait to celebrate with you!</p>"),
                TriggerType = TriggerType.ScheduledRelative,
                OffsetDays = -7,
                RelativeTo = "WeddingDate",
                AudienceType = AudienceType.Attending,
                MaxSendsPerUser = 1,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Gift Purchase Reminder",
                Subject = "Reminder: don't forget to buy {{ gift.name }}",
                HtmlBody = Wrap("Gift Reminder",
                    "<p>Hi {{ user.firstName }},</p>" +
                    "<p>Just a friendly reminder — you've reserved <strong>{{ gift.name }}</strong>.</p>" +
                    "{% if gift.purchaseLink %}" +
                    "<p><a href=\"{{ gift.purchaseLink }}\" style=\"color:#1a1a2e\">Purchase it here</a></p>" +
                    "{% endif %}" +
                    "<p>The wedding is on <strong>{{ wedding.date }}</strong>.</p>"),
                TriggerType = TriggerType.OnEvent,
                EventName = "GiftReservedReminder",
                AudienceType = AudienceType.TriggeredUser,
                MaxSendsPerUser = 1,
                IsActive = true
            },

            // ── Nudge / bulk ──────────────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Name = "No-RSVP Nudge",
                Subject = "Don't forget to RSVP!",
                HtmlBody = Wrap("RSVP Reminder",
                    "<p>Hi {{ user.firstName }},</p>" +
                    "<p>The wedding is coming up and we'd love to know if you'll be joining us.</p>" +
                    "<p>Please log in and submit your RSVP when you get a moment.</p>" +
                    "<p>Wedding date: <strong>{{ wedding.date }}</strong></p>"),
                TriggerType = TriggerType.ScheduledRelative,
                OffsetDays = -30,
                RelativeTo = "WeddingDate",
                AudienceType = AudienceType.NoRsvp,
                MaxSendsPerUser = 1,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Gift Registry Nudge",
                Subject = "Have you checked the gift registry?",
                HtmlBody = Wrap("Gift Registry",
                    "<p>Hi {{ user.firstName }},</p>" +
                    "<p>The wedding is in just {{ countdown.days }} days!</p>" +
                    "<p>Check out the gift registry on the wedding website and reserve something you'd like to contribute.</p>"),
                TriggerType = TriggerType.ScheduledRelative,
                OffsetDays = -14,
                RelativeTo = "WeddingDate",
                AudienceType = AudienceType.NoGiftReservation,
                MaxSendsPerUser = 1,
                IsActive = true
            }
        };

        context.EmailTemplates.AddRange(templates);
        context.SaveChanges();

        Console.WriteLine($"  {templates.Count} default email templates seeded");
    }
}
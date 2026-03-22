using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WeddingApp_Test.API.BackgroundServices;
using WeddingApp_Test.API.Filters;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Application.Mappings;
using WeddingApp_Test.Application.Services;
using WeddingApp_Test.Infrastructure.Data;
using WeddingApp_Test.Infrastructure.Persistence;
using WeddingApp_Test.Infrastructure.Repositories;
using WeddingApp_Test.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Module licensing — registers the filter that enforces [RequiresModule] on controllers.
builder.Services.AddScoped<ModuleEnforcementFilter>();
builder.Services.AddControllers(options =>
{
    // Register as a global filter so it runs on every controller action automatically.
    options.Filters.AddService<ModuleEnforcementFilter>();
});

// Bind the "Modules" section from appsettings.json to ModulesOptions.
// IOptions<ModulesOptions> is then available for injection (e.g. in ModuleEnforcementFilter and FeaturesController).
builder.Services.Configure<ModulesOptions>(
    builder.Configuration.GetSection(ModulesOptions.SectionName));

// CORS — allows the Angular dev server (port 4200) to call the backend.
// Without this, the browser blocks the cross-origin request and the app fails to start.
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()));

// Choose DB provider
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");
if (useInMemory)
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("InMemoryDb"));
}
else
{
	builder.Services.AddDbContext<AppDbContext>(options =>
		options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
}

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWeddingInfoRepository, WeddingInfoRepository>();
builder.Services.AddScoped<IRsvpRepository, RsvpRepository>();
builder.Services.AddScoped<IGiftRepository, GiftRepository>();
builder.Services.AddScoped<IReminderRepository, ReminderRepository>();


// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IWeddingInfoService, WeddingInfoService>();
builder.Services.AddScoped<IRsvpService, RsvpService>();
builder.Services.AddScoped<IGiftService, GiftService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
// Email — bind provider options
builder.Services.Configure<ResendOptions>(builder.Configuration.GetSection(ResendOptions.SectionName));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
// Register providers as IEmailProvider (order = priority: first = primary)
builder.Services.AddHttpClient<ResendEmailProvider>();
builder.Services.AddTransient<IEmailProvider, ResendEmailProvider>();
builder.Services.AddTransient<IEmailProvider, SmtpEmailProvider>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IReminderProcessor, ReminderProcessor>();

// Background Services
builder.Services.AddHostedService<ReminderBackgroundService>();

// AutoMapper
builder.Services.AddAutoMapper(
    cfg => { },  // prazna konfiguracija
    typeof(AutoMapperProfile).Assembly
);

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["TokenConfig:Issuer"],
        ValidAudience = builder.Configuration["TokenConfig:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["TokenConfig:Token"]!))
    };
});
builder.Services.AddAuthorization();

// Adding Swagger to project
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Data seeding
if (app.Environment.IsDevelopment())
{
    var isTestEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing";

    if (!isTestEnvironment)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        Console.WriteLine("");
        Console.WriteLine("=== Seeding development data ===");
        Console.WriteLine("");
    
        // Seed essential data (Admin + WeddingInfo)
        DataSeeder.SeedDevelopmentData(context, passwordHasher, tokenService);
    
        Console.WriteLine("=== Seeding completed ===");
        Console.WriteLine("");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    var urls = app.Urls.FirstOrDefault() ?? "http://localhost:5155";
    Console.WriteLine($"Swagger UI Endpoint: {urls}/swagger");
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
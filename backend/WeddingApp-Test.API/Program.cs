using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WeddingApp_Test.API.BackgroundServices;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Application.Mappings;
using WeddingApp_Test.Application.Services;
using WeddingApp_Test.Infrastructure.Data;
using WeddingApp_Test.Infrastructure.Email;
using WeddingApp_Test.Infrastructure.Persistence;
using WeddingApp_Test.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

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


// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IWeddingInfoService, WeddingInfoService>();
builder.Services.AddScoped<IRsvpService, RsvpService>();
builder.Services.AddScoped<IGiftService, GiftService>();

// Email providers
builder.Services.Configure<SmtpConfig>(builder.Configuration.GetSection("EmailConfig:Smtp"));
builder.Services.AddHttpClient<EmailProviderResend>();
builder.Services.AddKeyedScoped<IEmailProvider, EmailProviderResend>("resend");
builder.Services.AddKeyedScoped<IEmailProvider, EmailProviderSmtp>("smtp");
builder.Services.AddScoped<IEmailProvider, EmailProviderFallback>();
builder.Services.AddScoped<ILiquidRenderer, LiquidRenderer>();
builder.Services.AddScoped<IEmailDispatchService, WeddingApp_Test.Infrastructure.Email.EmailDispatchService>();

// Countdown image generation (singleton — font loaded once at startup)
builder.Services.AddSingleton<WeddingApp_Test.API.Services.CountdownImageService>();

// Background services
builder.Services.AddHostedService<EmailSchedulerService>();


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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
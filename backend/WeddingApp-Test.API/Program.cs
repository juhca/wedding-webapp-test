using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WeddingApp_Test.Application.Common.Interfaces;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Application.Services;
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

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IWeddingInfoService, WeddingInfoService>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    var urls = app.Urls.FirstOrDefault() ?? "http://localhost:5155";
    Console.WriteLine($"Swagger UI Endpoint: {urls}/swagger");
}

// Data seeding
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwhordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
	var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

	//Ensure db exists
	context.Database.EnsureCreated();

    var admin = context.Users.FirstOrDefault(u => u.Role == WeddingApp_Test.Domain.Enums.UserRole.Admin);

	if (admin is null)
    {
        passwhordHasher.CreatePasswordHash("Admin123!", out var hash, out var salt);

        admin = new WeddingApp_Test.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            FirstName = "Dev",
            LastName = "Admin",
            Email = "admin@dev.local",
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = WeddingApp_Test.Domain.Enums.UserRole.Admin
        };

		context.Users.Add(admin);

        context.SaveChanges();
    }

    var devToken = tokenService.CreateJwtToken(admin);

	Console.WriteLine("========================================");
	Console.WriteLine(" DEV ADMIN TOKEN (copy into Swagger) ");
	Console.WriteLine(" Bearer " + devToken);
	Console.WriteLine("========================================");
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
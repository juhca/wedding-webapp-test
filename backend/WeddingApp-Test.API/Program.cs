using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

// Adding Swagger to project
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
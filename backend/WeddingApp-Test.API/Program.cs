using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Application.Common.Interfaces;
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

app.UseAuthorization();

app.MapControllers();

app.Run();
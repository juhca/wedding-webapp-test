var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Adding Swagger to my project
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
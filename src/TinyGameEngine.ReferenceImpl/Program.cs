using TinyGameEngine.Core.Extensions;
using TinyGameEngine.Core.Engine.Interfaces;
using TinyGameEngine.ReferenceImpl;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

// Add TinyGameEngine Core services (this configures all the necessary services)
builder.Services.AddTinyGameEngine(builder.Configuration);

// Register your specific game implementation
builder.Services.AddScoped<IGameEngine, FakeGame>();

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tiny Game Engine API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tiny Game Engine API v1");
        c.RoutePrefix = "swagger"; // This makes Swagger available at /swagger
    });
}

// Configure the HTTP request pipeline using Core extension
app.UseTinyGameEngine();

app.Run();

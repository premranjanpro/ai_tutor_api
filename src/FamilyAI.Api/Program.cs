using FamilyAI.Api.Middlewares;
using FamilyAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/family_ai_log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Family AI Companion Web API");

    // Add Services to the container
    builder.Services.AddControllers();
    
    // Register DbContext conditionally
    var dbProvider = builder.Configuration.GetValue<string>("DbProvider") ?? "PostgreSQL";
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        if (dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            options.UseInMemoryDatabase("FamilyAIDb");
        }
        else
        {
            var connectionString = builder.Configuration.GetConnectionString("Default");
            options.UseNpgsql(connectionString, o => o.UseVector());
        }
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { } // Expose the Program class for integration testing

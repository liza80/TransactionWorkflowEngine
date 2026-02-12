using Microsoft.EntityFrameworkCore;
using TransactionWorkflowEngine.Data;
using TransactionWorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Database - SQL Server LocalDB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(3)));

// Memory Cache for workflow caching
builder.Services.AddMemoryCache();

// Application Services
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddSingleton<IWorkflowCacheService, WorkflowCacheService>();

// Controllers
builder.Services.AddControllers();

// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations and seed data automatically in development
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Database migrated successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database");
        // Continue running the app even if migration fails (might be already applied)
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Transaction Workflow Engine API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("Health");

app.Run();

// Make Program class public for integration tests
public partial class Program { }

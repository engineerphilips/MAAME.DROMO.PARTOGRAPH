using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=DATABASE-SERVER;Port=1433;InitialCatalog=MAAMEDROMODb;IntegratedSecurity=false;User=sa;Password=.server1;TrustServerCertificate=true;"; // "Server=database-server;Database=MAAMEBROMODb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False";
builder.Services.AddDbContext<PartographDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register services
builder.Services.AddScoped<IPartographPdfService, PartographPdfService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Configure CORS for mobile app access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add JSON serialization options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Partograph Service API",
        Version = "v2",
        Description = "API for Partograph mobile app synchronization, data management, and analytics for external web applications"
    });
});

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PartographDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Partograph Service API v1");
    });
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowMobileApp");

app.UseAuthorization();

app.MapControllers();

app.Run();

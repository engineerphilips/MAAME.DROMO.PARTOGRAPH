using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Database - prefer environment variable, then configuration
//var connectionString = Environment.GetEnvironmentVariable("PARTOGRAPH_DB_CONNECTION")
//    ?? builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string not configured. " +
        "Set PARTOGRAPH_DB_CONNECTION environment variable or configure ConnectionStrings:DefaultConnection in appsettings.");
}

builder.Services.AddDbContext<PartographDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register services
builder.Services.AddScoped<IPartographPdfService, PartographPdfService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRobsonService, RobsonService>();
builder.Services.AddScoped<IPOCMetricsService, POCMetricsService>();

//// Configure JWT Authentication
//var jwtSecretKey = Environment.GetEnvironmentVariable("PARTOGRAPH_JWT_SECRET")
//    ?? builder.Configuration["JwtSettings:SecretKey"];

//if (string.IsNullOrEmpty(jwtSecretKey) || jwtSecretKey.Length < 32)
//{
//    throw new InvalidOperationException(
//        "JWT Secret Key not configured or too short (minimum 32 characters). " +
//        "Set PARTOGRAPH_JWT_SECRET environment variable or configure JwtSettings:SecretKey in appsettings.");
//}

//var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "PartographSyncService";
//var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "PartographMobileApp";

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = jwtIssuer,
//        ValidAudience = jwtAudience,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
//        ClockSkew = TimeSpan.FromMinutes(5)
//    };

//    options.Events = new JwtBearerEvents
//    {
//        OnAuthenticationFailed = context =>
//        {
//            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
//            logger.LogWarning("Authentication failed: {Message}", context.Exception.Message);
//            return Task.CompletedTask;
//        }
//    };
//});

//builder.Services.AddAuthorization();

//// Configure CORS - restrict based on environment
//var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy.AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(origin => true)
        .AllowCredentials();
        //if (builder.Environment.IsDevelopment() ||
        //    (allowedOrigins.Length == 1 && allowedOrigins[0] == "*"))
        //{
        //    // Development mode - allow any origin for testing
        //    policy.AllowAnyOrigin()
        //          .AllowAnyMethod()
        //          .AllowAnyHeader();
        //}
        //else if (allowedOrigins.Length > 0)
        //{
        //    // Production mode - restrict to configured origins
        //    policy.WithOrigins(allowedOrigins)
        //          .AllowAnyMethod()
        //          .AllowAnyHeader()
        //          .AllowCredentials();
        //}
        //else
        //{
        //    // No origins configured - deny all cross-origin requests
        //    policy.SetIsOriginAllowed(_ => false);
        //}
    });
});

// Add JSON serialization options
builder.Services.AddControllers();
    //.AddJsonOptions(options =>
    //{
    //    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    //    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    //});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Partograph Service API",
        Version = "v2",
        Description = "API for Partograph mobile app synchronization, data management, and analytics for external web applications"
    });

    //// Add JWT Bearer authentication to Swagger
    //c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    //{
    //    Name = "Authorization",
    //    Type = SecuritySchemeType.Http,
    //    Scheme = "bearer",
    //    BearerFormat = "JWT",
    //    In = ParameterLocation.Header,
    //    Description = "Enter your JWT token"
    //});

    //c.AddSecurityRequirement(new OpenApiSecurityRequirement
    //{
    //    {
    //        new OpenApiSecurityScheme
    //        {
    //            Reference = new OpenApiReference
    //            {
    //                Type = ReferenceType.SecurityScheme,
    //                Id = "Bearer"
    //            }
    //        },
    //        Array.Empty<string>()
    //    }
    //});
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

    // Seed data for regions, districts, and monitoring users
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();
    var dataSeeder = new DataSeeder(dbContext, logger);
    dataSeeder.SeedAllDataAsync().GetAwaiter().GetResult();

    // Classify existing deliveries using WHO Robson Classification
    var robsonLogger = scope.ServiceProvider.GetRequiredService<ILogger<RobsonDataSeeder>>();
    var robsonSeeder = new RobsonDataSeeder(dbContext, robsonLogger);
    robsonSeeder.ClassifyExistingDeliveriesAsync().GetAwaiter().GetResult();

    // Generate sample data if no classifications exist (for demo purposes)
    if (!dbContext.RobsonClassifications.Any())
    {
        robsonSeeder.GenerateSampleDataAsync(500).GetAwaiter().GetResult();
    }
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

//app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowMobileApp");

// Authentication must come before Authorization
//app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

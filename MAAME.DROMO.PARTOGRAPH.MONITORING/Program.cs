using MAAME.DROMO.PARTOGRAPH.MONITORING.Services;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Hubs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Blazored.LocalStorage;
using Blazored.Toast;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Add response compression for SignalR
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

// Add Blazored LocalStorage for token persistence and offline queue
builder.Services.AddBlazoredLocalStorage();

// Add Blazored Toast for notifications
builder.Services.AddBlazoredToast();
builder.Services.AddScoped<IGlobalToastService, GlobalToastService>();

// Register Authorization Message Handler
builder.Services.AddScoped<AuthorizationMessageHandler>();

// Add HTTP client for API calls to the SERVICE project
builder.Services.AddHttpClient("PartographAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://192.168.100.4:5218");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

// Register HttpClient factory
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("PartographAPI"));

// Add authentication services (using API)
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add authorization with hierarchical policies
builder.Services.AddAuthorizationCore(options =>
{
    // National users can see everything
    options.AddPolicy("NationalAccess", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "AccessLevel" && c.Value == "National")));

    // Regional users can see their region and below
    options.AddPolicy("RegionalAccess", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "AccessLevel" &&
                (c.Value == "National" || c.Value == "Regional"))));

    // District users can see their district and below
    options.AddPolicy("DistrictAccess", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "AccessLevel" &&
                (c.Value == "National" || c.Value == "Regional" || c.Value == "District"))));

    // Admin role policy
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// Add monitoring services (all using HTTP client to call SERVICE API)
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IRegionService, RegionService>();
builder.Services.AddScoped<IDistrictService, DistrictService>();
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IUserService, UserService>();

// Enhanced Monitoring Services
builder.Services.AddScoped<ILiveLaborService, LiveLaborService>();
builder.Services.AddScoped<IEnhancedAlertService, EnhancedAlertService>();
builder.Services.AddScoped<IPredictiveAnalyticsService, PredictiveAnalyticsService>();
builder.Services.AddScoped<IDataQualityService, DataQualityService>();
builder.Services.AddScoped<IBenchmarkService, BenchmarkService>();
builder.Services.AddScoped<IAlertThresholdService, AlertThresholdService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IOfflineQueueService, OfflineQueueService>();
builder.Services.AddScoped<IReportVisualizationService, ReportVisualizationService>();

// CDS & Audit (Maturity Features)
builder.Services.AddScoped<ICDSService, CDSService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>(); // Placeholder registration

// Robson Classification Service (WHO 2017)
builder.Services.AddScoped<IRobsonClassificationService, RobsonClassificationService>();

// POC (Proof of Concept) Dashboard Service
builder.Services.AddScoped<IPOCDashboardService, POCDashboardService>();

// SignalR notification service
builder.Services.AddSingleton<IMonitoringNotificationService, MonitoringNotificationService>();

// Admin Services
builder.Services.AddScoped<IUserActivityService, UserActivityService>();
builder.Services.AddScoped<INotificationAdminService, NotificationAdminService>();
builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();
builder.Services.AddScoped<IRateLimitService, RateLimitService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapHub<MonitoringHub>("/monitoringhub");
app.MapFallbackToPage("/_Host");

app.Run();

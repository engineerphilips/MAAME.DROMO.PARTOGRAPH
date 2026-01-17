using MAAME.DROMO.PARTOGRAPH.MONITORING.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Blazored LocalStorage for token persistence
builder.Services.AddBlazoredLocalStorage();

// Add HTTP client for API calls to the SERVICE project
builder.Services.AddHttpClient("PartographAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://192.168.100.4:5218");

    //client.BaseAddress = new Uri("http://192.168.100.4:5218/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

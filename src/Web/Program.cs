using Core;
using Infrastructure;
using Scraper;
using Web;
using Web.Notifications;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

//file paths
var dataDir = builder.Configuration["Paths:DataDirectory"] 
              ?? Path.Combine(builder.Environment.ContentRootPath, "data");

Directory.CreateDirectory(dataDir);

var wishlistPath = Path.Combine(dataDir, "wishlist.json");
var seenPath = Path.Combine(dataDir, "seen.json");
var matchesPath = Path.Combine(dataDir, "matches.json");
var settingsPath = Path.Combine(dataDir, "settings.json");

// DBAHound services
builder.Services.AddSingleton<IWishlistRepository>(new JsonWishlistRepository(wishlistPath));
builder.Services.AddSingleton<ISeenListingsRepository>(new JsonSeenListingsRepository(seenPath));
builder.Services.AddSingleton<IMatchingService, MatchingService>();
builder.Services.AddHttpClient<IListingRepository, DbaListingRepository>();
builder.Services.AddSingleton<IMatchResultRepository>(new JsonMatchResultRepository(matchesPath));
builder.Services.AddHostedService<ScrapeScheduler>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
var settingsRepo = new JsonUserSettingsRepository(settingsPath);
if (!File.Exists(settingsPath))
{
    settingsRepo.Save(new UserSettings { 
        ScrapeIntervalHours = builder.Configuration.GetValue<int>("Scrape:IntervalHours", 24),
        NotificationProvider = builder.Configuration["Notifications:Provider"] ?? "None",
        PushoverToken = builder.Configuration["Notifications:Pushover:Token"] ?? "",
        PushoverUserKey = builder.Configuration["Notifications:Pushover:UserKey"] ?? "",
        NtfyUrl = builder.Configuration["Notifications:Ntfy:Url"] ?? "" });
}
builder.Services.AddSingleton<IUserSettingsRepository>(settingsRepo);

builder.Services.AddSingleton<IStatusCheckService, StatusCheckService>();
builder.Services.AddHostedService<StatusCheckScheduler>();

// Notification services
builder.Services.AddHttpClient<PushoverNotificationService>();
builder.Services.AddHttpClient<NtfyNotificationService>();
builder.Services.AddSingleton<DynamicNotificationService>();
builder.Services.AddSingleton<INotificationService>(sp => sp.GetRequiredService<DynamicNotificationService>());

builder.Services.AddSingleton<IUserSettingsRepository>(settingsRepo);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();
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

// DBAHound services
builder.Services.AddSingleton<IWishlistRepository>(new JsonWishlistRepository(wishlistPath));
builder.Services.AddSingleton<ISeenListingsRepository>(new JsonSeenListingsRepository(seenPath));
builder.Services.AddSingleton<IMatchingService, MatchingService>();
builder.Services.AddHttpClient<IListingRepository, DbaListingRepository>();
builder.Services.AddSingleton<IMatchResultRepository>(new JsonMatchResultRepository(matchesPath));
builder.Services.AddHostedService<ScrapeScheduler>();
var notificationProvider = builder.Configuration["Notifications:Provider"];
if (notificationProvider == "Ntfy")
    builder.Services.AddHttpClient<INotificationService, NtfyNotificationService>();
else
    builder.Services.AddHttpClient<INotificationService, PushoverNotificationService>();

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
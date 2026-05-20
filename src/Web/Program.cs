using Core;
using Infrastructure;
using Scraper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// DBAHound services
var wishlistPath = builder.Configuration["Paths:WishlistFile"] ?? "data/wishlist.json";
var seenPath = builder.Configuration["Paths:SeenFile"] ?? "data/seen.json";

builder.Services.AddSingleton<IWishlistRepository>(new JsonWishlistRepository(wishlistPath));
builder.Services.AddSingleton<ISeenListingsRepository>(new JsonSeenListingsRepository(seenPath));
builder.Services.AddSingleton<IMatchingService, MatchingService>();
builder.Services.AddHttpClient<IListingRepository, DbaListingRepository>();


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
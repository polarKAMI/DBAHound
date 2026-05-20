using System.Text.Json;
using Core;
using HtmlAgilityPack;

namespace Scraper;

public class DbaListingRepository : IListingRepository
{
    private readonly HttpClient _httpClient;
    private readonly ISeenListingsRepository _seenListings;
    private const int ScrapeDelayMs = 300;
    
    public DbaListingRepository(HttpClient httpClient, ISeenListingsRepository seenListings)
    {
        _httpClient = httpClient;
        _seenListings = seenListings;
    }

    public async Task<IEnumerable<Listing>> GetByPlatform(Platform platform)
    {
        var newIds = new List<int>();
        var page = 1;
        var stopPaging = false;

        while (!stopPaging)
        {
            var url = $"https://www.dba.dk/recommerce/forsale/search?games_group={MapPlatformId(platform)}&product_category=2.93.3905.64&page={page}&sort=PUBLISHED_DESC";
            var html = await _httpClient.GetStringAsync(url);
        
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var nodes = document.DocumentNode
                .SelectNodes("//a[contains(@href, '/recommerce/forsale/item/')]");

            if (nodes == null)
            {
                stopPaging = true;
                break;
            }
        
            var pageIds = nodes
                .Select(n => n.GetAttributeValue("href", "").Split('/')[^1])
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .Distinct()
                .ToList();

            foreach (var id in pageIds)
            {
                if (_seenListings.Contains(id))
                {
                    stopPaging = true;
                    break;
                }
                newIds.Add(id);
            }

            if (!stopPaging)
                page++;
        }

        var listings = new List<Listing>();
        foreach (var id in newIds)
        {
            try
            {
                var listing = await GetById(id);
                listings.Add(listing);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed for id {id}: {ex.Message}");
            }
            await Task.Delay(ScrapeDelayMs);
        }

        _seenListings.AddRange(newIds);
        return listings;
    }

    public async Task<Listing> GetById(int id)
    {
        var url = $"https://www.dba.dk/recommerce/forsale/item/{id}";
        var html = await _httpClient.GetStringAsync(url);
    
        var document = new HtmlDocument();
        document.LoadHtml(html);
    
        var scriptNode = document.DocumentNode
            .SelectSingleNode("//script[contains(text(),'__staticRouterHydrationData')]");
    
        if (scriptNode == null)
            throw new Exception($"Could not find hydration data for listing {id}");
    
        string jsonStart = "JSON.parse(\"";
        int startIndex = scriptNode.InnerText.IndexOf(jsonStart);
        startIndex += jsonStart.Length;

        string jsonEnd = "\");";
        int endIndex = scriptNode.InnerText.IndexOf(jsonEnd);

        string json = scriptNode.InnerText.Substring(startIndex, endIndex - startIndex);
        json = json.Replace("\\\"", "\"").Replace("\\\\", "\\");

        var jsonDocument = JsonDocument.Parse(json);

        if (!jsonDocument.RootElement.TryGetProperty("loaderData", out var loaderData))
            throw new Exception($"Missing loaderData for listing {id}");
    
        if (!loaderData.TryGetProperty("item-recommerce", out var itemRecommerce))
            throw new Exception($"Missing item-recommerce for listing {id}");
    
        if (!itemRecommerce.TryGetProperty("itemData", out var itemData))
            throw new Exception($"Missing itemData for listing {id}");

        var adId = itemData.GetProperty("meta").GetProperty("adId").GetString();
        var title = itemData.GetProperty("title").GetString();

        var description = itemData.TryGetProperty("description", out var descProp)
            ? descProp.GetString()
            : null;

        var imageUrl = itemData.TryGetProperty("images", out var imagesProp) && imagesProp.GetArrayLength() > 0
            ? imagesProp[0].TryGetProperty("uri", out var uriProp) ? uriProp.GetString() : null
            : null;

        var extras = itemData.TryGetProperty("extras", out var extrasProp)
            ? extrasProp.EnumerateArray().ToList()
            : new List<JsonElement>();

        var condition = extras.Any(e => e.GetProperty("id").GetString() == "condition")
            ? extras.First(e => e.GetProperty("id").GetString() == "condition")
                .GetProperty("value").GetString()
            : null;

        var platformExtra = extras.FirstOrDefault(e => e.GetProperty("id").GetString() == "games_group");
        var platform = platformExtra.ValueKind != JsonValueKind.Undefined
            ? MapPlatform(platformExtra.GetProperty("valueId").GetInt32())
            : Platform.PS2;

        var price = itemData.TryGetProperty("price", out var priceProp)
            ? priceProp.GetDecimal()
            : 0m;

        var lastEdit = itemData.TryGetProperty("meta", out var metaProp) &&
                    metaProp.TryGetProperty("edited", out var editProp)
            ? editProp.GetString()
            : null;

        var postalCode = itemData.TryGetProperty("location", out var locationProp) 
            ? locationProp.GetProperty("postalCode").GetString()
            : null;

        var postalName = itemData.TryGetProperty("location", out var locationProp2)
            ? locationProp2.GetProperty("postalName").GetString()
            : null;

        return new Listing
        {
            Id = int.Parse(adId),
            Title = title,
            Description = description,
            ImageUrl = imageUrl,
            Condition = condition,
            Price = price,
            Platform = platform,
            LastEdited = lastEdit != null ? DateTime.Parse(lastEdit) : null,
            PostalCode = postalCode,
            PostalName = postalName,
            IsLot = null
        };
    }
    private Platform MapPlatform(int valueId) => valueId switch
    {
        2 => Platform.PS1,
        13 => Platform.PS2,
        11 => Platform.Switch,
        12 => Platform.Wii,
        8 => Platform.ThreeDS,
        7 => Platform.DS,
        _ => throw new Exception($"Unknown platform id {valueId}")
    };

    private int MapPlatformId(Platform platform) => platform switch
    {
        Platform.PS1 => 2,
        Platform.PS2 => 13,
        Platform.Switch => 11,
        Platform.Wii => 12,
        Platform.ThreeDS => 8,
        Platform.DS => 7,
        _ => throw new Exception($"Unknown platform id {platform}")
    };
}
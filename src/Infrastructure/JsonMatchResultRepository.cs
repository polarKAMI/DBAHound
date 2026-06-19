using System.Text.Json;
using Core;

namespace Infrastructure;

public class JsonMatchResultRepository : IMatchResultRepository
{
    private readonly string _filePath;

    public JsonMatchResultRepository(string filePath)
    {
        _filePath = filePath;
    }
    
    public IEnumerable<StoredMatch> GetAll()
    {
        if (!File.Exists(_filePath))
            return new List<StoredMatch>();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<StoredMatch>>(json) ?? new List<StoredMatch>();
    }

    public void AddRange(IEnumerable<StoredMatch> matches)
    {
        var storedMatches = GetAll().ToList();

        storedMatches.AddRange(matches);

        var json = JsonSerializer.Serialize(storedMatches);
        File.WriteAllText(_filePath, json);
    }
    
    public void Clear()
    {
        File.WriteAllText(_filePath, JsonSerializer.Serialize(new List<StoredMatch>()));
    }
    
    public void Dismiss(int listingId)
    {
        var matches = GetAll().ToList();
        var match = matches.FirstOrDefault(m => m.ListingId == listingId);
        if (match != null)
        {
            match.IsDismissed = true;
            File.WriteAllText(_filePath, JsonSerializer.Serialize(matches));
        }
    }

    public void ToggleFavourite(int listingId)
    {
        var matches = GetAll().ToList();
        var match = matches.FirstOrDefault(m => m.ListingId == listingId);
        if (match != null)
        {
            match.IsFavourite = !match.IsFavourite;
            File.WriteAllText(_filePath, JsonSerializer.Serialize(matches));
        }
    }
    
    public void Restore(int listingId)
    {
        var all = GetAll().ToList();
        var match = all.FirstOrDefault(m => m.ListingId == listingId);
        if (match != null)
        {
            match.IsDismissed = false;
            File.WriteAllText(_filePath, JsonSerializer.Serialize(all));
        }
    }
    
    public void Cleanup(int keepDays)
    {
        var all = GetAll().ToList();
        var cutoff = DateTime.Now.AddDays(-keepDays);
        var cleaned = all.Where(m => 
            m.IsFavourite || 
            m.IsDismissed || 
            m.FoundAt >= cutoff).ToList();
        File.WriteAllText(_filePath, JsonSerializer.Serialize(cleaned));
    }
}
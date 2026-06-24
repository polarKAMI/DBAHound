using System.Text.Json;
using Core;

namespace Infrastructure;

public class JsonSeenListingsRepository : ISeenListingsRepository
{
    private readonly string _filePath;
    private Dictionary<string, List<int>> _seenIds;

    public JsonSeenListingsRepository(string filePath)
    {
        _filePath = filePath;
        _seenIds = Load();
        // Temporary debug
        foreach (var kvp in _seenIds)
            Console.WriteLine($"Loaded {kvp.Value.Count} seen IDs for {kvp.Key}");
    }

    private Dictionary<string, List<int>> Load()
    {
        if (!File.Exists(_filePath))
            return new Dictionary<string, List<int>>();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<Dictionary<string, List<int>>>(json)
               ?? new Dictionary<string, List<int>>();
    }

    private void Save()
    {
        File.WriteAllText(_filePath, JsonSerializer.Serialize(_seenIds));
    }

    private List<int> GetPlatformList(Platform platform)
    {
        var key = platform.ToString();
        if (!_seenIds.ContainsKey(key))
            _seenIds[key] = new List<int>();
        return _seenIds[key];
    }

    public bool Contains(int id, Platform platform)
    {
        return GetPlatformList(platform).Contains(id);
    }

    public void AddRange(IEnumerable<int> ids, Platform platform)
    {
        var list = GetPlatformList(platform);
        var newList = ids.ToList();
        newList.AddRange(list);
        _seenIds[platform.ToString()] = newList;
        Save();
    }

    public void CleanUp(Platform platform, int keepCount = 500)
    {
        var list = GetPlatformList(platform);
        var trimmed = list.Take(keepCount).ToList();
        _seenIds[platform.ToString()] = trimmed;
        Save();
    }

    public void CleanUpAll(int keepCount = 500)
    {
        foreach (var key in _seenIds.Keys.ToList())
        {
            _seenIds[key] = _seenIds[key].Take(keepCount).ToList();
        }
        Save();
    }

    public void ClearPlatform(Platform platform)
    {
        _seenIds[platform.ToString()] = new List<int>();
        Save();
    }
}
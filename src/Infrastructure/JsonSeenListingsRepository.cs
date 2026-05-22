using System.Text.Json;
using Core;

namespace Infrastructure;

public class JsonSeenListingsRepository : ISeenListingsRepository
{
    private readonly string _filePath;
    private readonly List<int> _seenIds;

    public JsonSeenListingsRepository(string filePath)
    {
        _filePath = filePath;
        _seenIds = File.Exists(filePath)
            ? JsonSerializer.Deserialize<List<int>>(File.ReadAllText(filePath)) ?? new List<int>()
            : new List<int>();
    }

    public bool Contains(int id)
    {
        return _seenIds.Contains(id);
    }

    public void AddRange(IEnumerable<int> ids)
    {
        _seenIds.AddRange(ids);
        var json = JsonSerializer.Serialize(_seenIds);
        File.WriteAllText(_filePath, json);
    }

    public void CleanUp(int keepCount = 500)
    {
        var trimmed = _seenIds.TakeLast(keepCount).ToList();
        _seenIds.Clear();
        _seenIds.AddRange(trimmed);
        File.WriteAllText(_filePath, JsonSerializer.Serialize(_seenIds));
    }
}
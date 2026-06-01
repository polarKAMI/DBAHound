using System.Text.Json;
using Core;

namespace Infrastructure;

public class JsonUserSettingsRepository : IUserSettingsRepository
{
    private readonly string _filePath;

    public JsonUserSettingsRepository(string filePath)
    {
        _filePath = filePath;
    }

    public UserSettings Get()
    {
        if (!File.Exists(_filePath))
            return new UserSettings();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
    }

    public void Save(UserSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
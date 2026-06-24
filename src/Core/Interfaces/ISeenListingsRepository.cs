namespace Core;

public interface ISeenListingsRepository
{
    bool Contains(int id, Platform platform);
    void AddRange(IEnumerable<int> ids, Platform platform);
    void CleanUp(Platform platform, int keepCount = 500);
    void CleanUpAll(int keepCount = 500);
    void ClearPlatform(Platform platform);
}
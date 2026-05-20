namespace Core;

public interface ISeenListingsRepository
{
    bool Contains(int id);
    void AddRange(IEnumerable<int> ids);
}
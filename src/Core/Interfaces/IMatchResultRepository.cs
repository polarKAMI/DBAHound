namespace Core;

public interface IMatchResultRepository
{
    IEnumerable<StoredMatch> GetAll();
    void AddRange(IEnumerable<StoredMatch> matches);
    void Clear();
    void Dismiss(int listingId);
    void ToggleFavourite(int listingId);

}
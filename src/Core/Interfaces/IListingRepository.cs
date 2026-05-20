namespace Core;

public interface IListingRepository
{
    Task<IEnumerable<Listing>> GetByPlatform(Platform platform);
    Task<Listing> GetById(int id);
}
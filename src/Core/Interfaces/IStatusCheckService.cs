namespace Core;

public interface IStatusCheckService
{
    Task CheckAllAsync(CancellationToken cancellationToken);
}
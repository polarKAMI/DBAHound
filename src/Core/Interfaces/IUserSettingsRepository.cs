namespace Core;

public interface IUserSettingsRepository
{
    UserSettings Get();
    void Save(UserSettings settings);
}
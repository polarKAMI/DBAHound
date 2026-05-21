namespace Core;

public class AppSettings
{
    public PathSettings Paths { get; set; } = new();
}

public class PathSettings
{
    public string WishlistFile { get; set; } = string.Empty;
    public string SeenFile { get; set; } = string.Empty;
    public string MatchesFile { get; set; } = string.Empty;
}
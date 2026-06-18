using static System.Environment;

namespace BloodSugarWatchdog.Data;

internal static class EnvironmentPaths
{
    private static string LocalUserPath
        => GetFolderPath(SpecialFolder.UserProfile);

    public static string LocalDataPath
        => GetFolderPath(SpecialFolder.LocalApplicationData);

    public static string LocalCachePath
        => GetEnvironmentVariable("XDG_CACHE_HOME")
        ?? OSVersion.Platform switch
        {
            PlatformID.Unix
                => Path.Join(LocalUserPath, ".cache"),
            _
                => Path.Join(LocalDataPath, "cache"),
        };
}

namespace BloodSugarWatchdog.Nightscout;

internal sealed class NightscoutOptions
{
    public string Username { get; set; } = string.Empty;
    public string ClientUserAgent { get; set; } = "BloodSugarWatchdog/1.0";
}

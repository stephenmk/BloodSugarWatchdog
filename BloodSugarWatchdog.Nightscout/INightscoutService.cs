namespace BloodSugarWatchdog.Nightscout;

public interface INightscoutService
{
    Task RunAsync(int millisecondsDelay, CancellationToken ct = default);
}

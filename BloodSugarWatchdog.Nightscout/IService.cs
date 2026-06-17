namespace BloodSugarWatchdog.Nightscout;

public interface IService
{
    Task RunAsync(int millisecondsDelay, CancellationToken ct = default);
}

using BloodSugarWatchdog.Data;
using BloodSugarWatchdog.Import;

namespace BloodSugarWatchdog.Nightscout;

internal sealed class ServiceProvider
{
    public IService GetService(string username)
    {
        var context = new Context(username);
        context.Database.EnsureCreated();

        var client = new NightscoutClient(username);

        var bglImporter = new BglImporter(context);
        var treatmentImporter = new TreatmentImporter(context);

        var service = new Service(client, bglImporter, treatmentImporter);

        return service;
    }
}

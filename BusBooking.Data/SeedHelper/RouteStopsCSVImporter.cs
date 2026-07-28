using BusBooking.Models.Entities;

namespace BusBooking.Data.SeedHelper;

public class RouteStopsCSVImporter
{
    private readonly AppDbContext _db;

    public RouteStopsCSVImporter (AppDbContext db)
    {
        _db = db;
    }

    public void Import (string filepath)
    {
        if (!File.Exists(filepath))
        {
            Console.WriteLine($"File not found: {filepath}");
            return;
        }

        var csv = File.ReadAllLines(filepath).Skip(1);
        foreach (var line in csv)
        {
            if(string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(",");
            var routeId = int.Parse(parts[1].Trim());
            var busStopId = int.Parse(parts[2].Trim());
            

            var routeStop = new RouteStops
            {
                RouteId = routeId,
                BusStopId = busStopId
            };
            _db.RouteStops.Add(routeStop);
        }
        _db.SaveChanges();
    }
}
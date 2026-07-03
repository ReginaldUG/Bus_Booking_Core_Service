using BusBooking.Models.Entities;

namespace BusBooking.Data.SeedHelper;

public class RouteCSVImporter
{
    private readonly AppDbContext _db;
    public RouteCSVImporter(AppDbContext db)
    {
        _db = db;
    }
    public void Import(string filepath)
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
            var routeName = parts[1].Trim();
            var price = decimal.Parse(parts[2].Trim());

            var route = new Route
            {
                RouteName = routeName,
                Price = price
            };
            _db.Routes.Add(route);
        }
        _db.SaveChanges();
    }
}
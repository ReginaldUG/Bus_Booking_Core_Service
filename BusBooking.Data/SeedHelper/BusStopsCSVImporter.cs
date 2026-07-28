using BusBooking.Models.Entities;

namespace BusBooking.Data.SeedHelper;

public class BusStopsCSVImporter
{
    private readonly AppDbContext _db;

    public BusStopsCSVImporter(AppDbContext db)
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
            var name = parts[1].Trim();

            var busStop = new BusStops
            {
                Name = name
            };
            _db.BusStops.Add(busStop);
        }
        _db.SaveChanges();
    }

}
using BusBooking.Core.Enums;
using BusBooking.Models.Entities;

namespace BusBooking.Data.SeedHelper;

public class ScheduleRuleCSVImporter
{
    private readonly AppDbContext _db;

    public ScheduleRuleCSVImporter(AppDbContext db)
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

            var routeId = int.Parse(parts[1].Trim());
            var dayOfWeek = int.Parse(parts[2].Trim());
            var scheduledDepartureTime = TimeOnly.Parse(parts[3].Trim());
            var estimatedDuration = TimeSpan.Parse(parts[4].Trim());

            var rule = new ScheduleRules
            {
                RouteId = routeId,
                DayOfWeek = (WorkingDays)dayOfWeek,
                ScheduledDepartureTime = scheduledDepartureTime,
                EstimatedDuration = estimatedDuration
            };
            _db.ScheduleRules.Add(rule);
        }
        _db.SaveChanges();
    }
}
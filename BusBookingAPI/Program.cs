using BusBooking.Data;
using BusBooking.Services;
using BusBookingAPI.Helpers;
using Dapper;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options=>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b=>b.MigrationsAssembly("BusBooking.Migrations")
    ));

//  Registering Services
builder.Services.AddDataInjections(builder.Configuration);
builder.Services.AddServiceInjections(builder.Configuration);

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Routes.Any())
    {
        var importer = new BusBooking.Data.SeedHelper.RouteCSVImporter(db);
        importer.Import("Seed/routes.csv");
        db.SaveChanges();
    }
    if (!db.ScheduleRules.Any())
    {
        var importer = new BusBooking.Data.SeedHelper.ScheduleRuleCSVImporter(db);
        importer.Import("Seed/scheduleRules.csv");
        db.SaveChanges();
    }
}

if(!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
using System.Text;
using BusBooking.Data;
using BusBooking.Services;
using BusBookingAPI.Helpers;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
        };
    });

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
    if (!db.BusStops.Any())
    {
        var importer = new BusBooking.Data.SeedHelper.BusStopsCSVImporter(db);
        importer.Import("Seed/busStops.csv");
        db.SaveChanges();
    }
    if (!db.ScheduleRules.Any())
    {
        var importer = new BusBooking.Data.SeedHelper.ScheduleRuleCSVImporter(db);
        importer.Import("Seed/scheduleRules.csv");
        db.SaveChanges();
    }
    
    if (!db.RouteStops.Any())
    {
        var importer = new BusBooking.Data.SeedHelper.RouteStopsCSVImporter(db);
        importer.Import("Seed/routeStops.csv");
        db.SaveChanges();
    }
}

if(!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
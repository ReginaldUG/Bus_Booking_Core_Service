using BusBooking.Data;
using BusBooking.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
}

if(!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
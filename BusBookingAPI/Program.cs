using BusBooking.Data;
using BusBooking.Data.Executers;
using BusBooking.Data.Helpers;
using BusBooking.Data.Queries;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options=>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b=>b.MigrationsAssembly("BusBooking.Migrations")
    ));

builder.Services.AddScoped<ReadExecuter>();
builder.Services.AddScoped<ReadUtilities>();
builder.Services.AddScoped<WriteUtilities>();

// Registers the open generic QueryRepository for any class entity used
builder.Services.AddScoped(typeof(QueryRepository<>));

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
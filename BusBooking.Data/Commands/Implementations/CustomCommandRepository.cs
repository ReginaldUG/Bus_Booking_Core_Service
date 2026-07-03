using BusBooking.Core.Constants;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Executers.Interfaces;
using BusBooking.Data.Helpers.Interfaces;
using BusBooking.Models.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BusBooking.Data.Commands.Implementations;

public class CustomCommandRepository : ICustomCommandRepository
{
    private readonly string _connStr;
    private readonly IWriteUtilities _utilities;
    private readonly IWriteExecuter _executer;

    public CustomCommandRepository (IConfiguration configuration, IWriteUtilities utilities, IWriteExecuter executer)
    {
        _connStr = configuration.GetConnectionString("DefaultConnection") ?? "";
        _utilities = utilities;
        _executer = executer;
    }

    public async Task UpdateAssignBusesToScheduleAsync(int id, Bus bus, NpgsqlTransaction transaction)
    {
        var updateQuery = $"UPDATE \"Schedules\" SET \"{nameof(Schedule.BusId)}\" = @BusId, \"{nameof(Schedule.AvailableSeats)}\" = @AvailableSeats, \"{nameof(Schedule.Status)}\" = @Status WHERE \"{nameof(Schedule.Id)}\" = @Id;";
        await _executer.ExecuteCommandAsync(updateQuery, new { 
            BusId = bus.Id,
            AvailableSeats = bus.SeatCapacity,
            Status = ScheduleStatus.Scheduled,
            id
        }, transaction);
    }

    //GENERIC Schedule update function that skips over DateOfDeparture
    public async Task UpdateQueryAsync()
    {
        
    }
}
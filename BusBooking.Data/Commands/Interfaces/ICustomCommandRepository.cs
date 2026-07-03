using BusBooking.Models.Entities;
using Npgsql;

namespace BusBooking.Data.Commands.Interfaces;

public interface ICustomCommandRepository
{
    Task UpdateAssignBusesToScheduleAsync(int id, Bus bus, NpgsqlTransaction transaction);
}
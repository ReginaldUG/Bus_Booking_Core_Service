using BusBooking.Models.DTO;

namespace BusBooking.Services.BL.Interfaces;

public interface IAccountEvaluationService
{
    Task<ApiResponse> DriverActivationServiceTask();

}
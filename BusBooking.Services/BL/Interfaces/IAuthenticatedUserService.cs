namespace BusBooking.Services.BL.Interfaces;

public interface IAuthenticatedUserService
{
    public int? UserId { get; }
    public string? Email { get; }
}
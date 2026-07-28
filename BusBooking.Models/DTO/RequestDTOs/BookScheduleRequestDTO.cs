namespace BusBooking.Models.DTO.RequestDTOs;

public class BookScheduleRequestDTO
{
    public string Token { get; set; }
    public int ScheduleId { get; set; }
    public int PickUpStopId { get; set; }
    public int DropOffStopId { get; set; }
    public int NumberOfSeats { get; set; }
}
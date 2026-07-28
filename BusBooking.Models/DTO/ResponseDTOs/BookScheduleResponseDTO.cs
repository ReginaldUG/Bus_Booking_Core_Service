namespace BusBooking.Models.DTO.ResponseDTOs;

public class BookScheduleResponseDTO
{
    public int ScheduleID { get; set; }    
    public TimeOnly DepartureTime { get; set; }
    public TimeOnly ArrivaleTime { get; set; }
    public string CustomerName { get; set; }
    public string BusPlateNumber { get; set; }
}
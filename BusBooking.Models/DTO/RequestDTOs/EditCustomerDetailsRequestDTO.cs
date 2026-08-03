using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models.DTO.RequestDTOs;

public class EditCustomerDetailsRequestDTO
{
        public string? FirstName { get; set; }
        public string? LastName {get; set; }
        public string? Email {get; set; }
        public string? Age {get; set; }
        public string? PhoneNumber { get; set; }
}
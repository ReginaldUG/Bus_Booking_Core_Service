namespace BusBooking.Core.Attributes;
public class WriteTableNameAttribute : Attribute
{
    public WriteTableNameAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
    
}
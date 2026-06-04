namespace BusBooking.Data.Helpers;

public class WriteUtilities
{
    private readonly AppDbContext _context;

    public WriteUtilities(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
}

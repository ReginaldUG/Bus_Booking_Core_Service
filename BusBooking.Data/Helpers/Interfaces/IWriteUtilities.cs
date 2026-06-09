namespace BusBooking.Data.Helpers.Interfaces;

public interface IWriteUtilities
{
    string GenerateInsertQuery<TEntity>() where TEntity : class;
    string GenerateUpdateQuery<TEntity>() where TEntity : class;
}
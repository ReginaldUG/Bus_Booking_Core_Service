namespace BusBooking.Data.Queries.Interfaces;

public interface IQueryRepository<TEntity> where TEntity : class
{
    Task<TEntity?> FindByIdAsync(int id);
    Task<IEnumerable<TEntity>> GetAllAsync(int pageSize, int pageNumber);
    Task<TEntity?> FindByCriteriaAsync(string propertyName, string value);
    Task<long> SearchCountInAsync(KeyValuePair<String, List<string>> searchParams);
}
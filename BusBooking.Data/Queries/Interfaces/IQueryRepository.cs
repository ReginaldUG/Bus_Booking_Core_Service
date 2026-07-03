namespace BusBooking.Data.Queries.Interfaces;

public interface IQueryRepository<TEntity> where TEntity : class
{
    Task<TEntity?> FindByIdAsync(int id);
    Task<IEnumerable<TEntity>> GetAllAsync(int pageSize, int pageNumber);
    Task<IEnumerable<TEntity>> FindAllByMultipleValuesAsync(string propertyName, IEnumerable<string> values);
    Task<TEntity?> FindByCriteriaAsync(string propertyName, string value);
    Task<IEnumerable<TEntity>> FindByMultipleFieldsAsync(Dictionary<string, object> criteria, int? queryLimit);
    Task<IEnumerable<TEntity>> GetAllByCriteriaAsync(string propertyName, string value);
    Task<IEnumerable<TEntity>> GetLimitedByCriteriaAsync(string propertyName, string value, int limit);
    Task<long> SearchCountInAsync(KeyValuePair<String, List<string>> searchParams);
    //Task<long> GetScheduleExistsCount(string propertyName, DateTime dayStart, DateTime dayEnd);
}
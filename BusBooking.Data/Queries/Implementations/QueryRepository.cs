using BusBooking.Data.Executers.Interfaces;
using BusBooking.Data.Helpers.Interfaces;
using BusBooking.Data.Queries.Interfaces;

namespace BusBooking.Data.Queries.Implementations;

public class QueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly string _connStr;
    private readonly IReadUtilities _utilities;
    private readonly IReadExecuter _executer;
    
    public QueryRepository(IReadUtilities utilities, IReadExecuter executer)
    {
        _utilities = utilities;
        _executer = executer;
        _connStr = _utilities.GetConnectionString();
    }

    //Fecth by primary key/id
    public async Task<TEntity?> FindByIdAsync(int id)
    {
        var query = _utilities.GenerateSelectSingleRecordQuery<TEntity>("Id", id.ToString());
        var entities = await _executer.ExecuteReaderAsync<TEntity>(_connStr, query, null);
        return entities.FirstOrDefault();
    }

    //Fetch all records with pagination
    public async Task<IEnumerable<TEntity>> GetAllAsync(int pageSize, int pageNumber)
    {
        var query = _utilities.GenerateSelectQuery<TEntity>(pageSize, pageNumber);
        var entities = await _executer.ExecuteReaderAsync<TEntity>(_connStr, query, null);
        return entities;        
    }

    public async Task<IEnumerable<TEntity>> GetAllByCriteriaAsync(string propertyName, string value)
    {
        var query = _utilities.GenerateSelectMultipleRecordsQuery<TEntity>(propertyName, value);
        var entities = await _executer.ExecuteReaderAsync<TEntity>(_connStr, query, param: null);
        return entities;
    }

    public async Task<IEnumerable<TEntity>> GetLimitedByCriteriaAsync(string propertyName, string value, int limit)
    {
        var query = _utilities.GenerateSelectLimitedRecordsQuery<TEntity>(propertyName, value, limit);
        var entities = await _executer.ExecuteReaderAsync<TEntity>(_connStr, query, param: null);
        return entities;
    }

    public async Task<IEnumerable<TEntity>> FindAllByMultipleValuesAsync (string propertyName, IEnumerable<string> values)
    {
        var cleanValues = values.Select(v => v.Trim()).ToArray();

        var query = _utilities.GenerateSelectByMultipleValuesListQuery<TEntity>(propertyName, cleanValues);
        
        var entities = await _executer.ExecuteReaderAsync<TEntity>(_connStr, query, param: null);
        return entities.ToList();
    }

    //Fetch single record by any single search criteria
    public async Task<TEntity?> FindByCriteriaAsync(string propertyName, string value)
    {
        var query = _utilities.GenerateSelectSingleRecordQuery<TEntity>(propertyName, value);
        var entities = await _executer.ExecuteReaderAsync<TEntity>(_connStr, query, null);
        return entities.FirstOrDefault();
    }
    
    //Fetch number of records that match a criteria
    public async Task<long> SearchCountInAsync(KeyValuePair<String, List<string>> searchParams)
    {
        var query = _utilities.GenerateSelectCountInAsync<TEntity>(searchParams);
        var response = await _executer.ExecuteReaderAsync<long>(_connStr, query, null);
        return response.FirstOrDefault();
    }
}

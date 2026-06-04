using BusBooking.Data.Executers;
using BusBooking.Data.Helpers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BusBooking.Data.Queries;

public class QueryRepository<TEntity> where TEntity : class
{
    private readonly string _connStr;
    private readonly ReadUtilities _utilities;
    private readonly ReadExecuter _executer;
    
    public QueryRepository(ReadUtilities utilities, ReadExecuter executer)
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

    //Fetch sinlge record by any single search criteria
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

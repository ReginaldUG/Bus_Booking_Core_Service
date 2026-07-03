using BusBooking.Data.Executers.Interfaces;
using BusBooking.Data.Extensions;
using BusBooking.Data.Helpers.Interfaces;
using BusBooking.Data.Queries.Interfaces;

namespace BusBooking.Data.Queries.Implementations;

public class CustomQueryRepository<TEntity> : ICustomQueryRepository<TEntity> where TEntity : class
{
    private readonly string _connStr;
    private readonly IReadUtilities _utilities;
    private readonly IReadExecuter _executer;
    
    public CustomQueryRepository(IReadUtilities utilities, IReadExecuter executer)
    {
        _utilities = utilities;
        _executer = executer;
        _connStr = _utilities.GetConnectionString();
    }

    
}
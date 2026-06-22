using System.Data;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Executers.Interfaces;
using BusBooking.Data.Helpers.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BusBooking.Data.Commands.Implementations;

public class CommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : class
{
    private readonly string _connStr;
    private readonly IWriteUtilities _utilities;
    private readonly IWriteExecuter _executer;

    public CommandRepository (IConfiguration configuration, IWriteUtilities utilities, IWriteExecuter executer)
    {
        _connStr = configuration.GetConnectionString("DefaultConnection") ?? "";
        _utilities = utilities;
        _executer = executer;
    }

    public async Task AddAsync(TEntity entity)
    {
        var query = _utilities.GenerateInsertQuery<TEntity>();
        await _executer.ExecuteCommandAsync(_connStr, query, entity);
    }

    public async Task UpdateAsync(TEntity entity)
    {
        var query = _utilities.GenerateUpdateQuery<TEntity>();
        await _executer.ExecuteCommandAsync(_connStr, query, entity);
    }

    public async Task<TEntity> AddWithOpenDBTransaction(TEntity entity, NpgsqlTransaction sqltransaction)
    {
        var query = _utilities.GenerateInsertQuery<TEntity>();
        var id = await _executer.ExecuteCommandAndReturnIdAsync(
            query,
            entity,
            sqltransaction
        );

        // Assign generated Id back to entity
        var idProperty = typeof(TEntity).GetProperty("Id");

        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(entity, id);
        }

        return entity;
    }

    public async Task UpdateWithOpenDbTransactionAsync(TEntity entity, NpgsqlTransaction sqltransaction)
    {
        var query = _utilities.GenerateUpdateQuery<TEntity>();
        await _executer.ExecuteCommandAsync(query, entity, sqltransaction);

    }

    public NpgsqlTransaction BeginTransaction()
    {
        var connection = new NpgsqlConnection(_connStr);
        connection.Open();
        var sqlTransaction = connection.BeginTransaction();
        return sqlTransaction;
    }
    
    public void CommitTransaction(NpgsqlTransaction sqlTransaction)
    {
        try
        {
            sqlTransaction.Commit();        
        }
        finally
        {
            sqlTransaction.Connection?.Close();
            sqlTransaction.Dispose();
        }
    }
    public void RollbackTransaction(NpgsqlTransaction sqlTransaction)
    {
        try
        {
            sqlTransaction.Rollback();
        }
        finally
        {
            sqlTransaction.Connection?.Close();
            sqlTransaction.Dispose();
        }
    }
}
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

    public async Task<int> AddWithOpenDBTransaction(TEntity entity, NpgsqlTransaction sqltransaction)
    {
        var query = _utilities.GenerateInsertQuery<TEntity>();
        return await _executer.ExecuteCommandAndReturnIdAsync(query, entity, sqltransaction);
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
            if (sqlTransaction.Connection != null)
            {
                using var conn = sqlTransaction.Connection;
                sqlTransaction.Commit();
                conn.Close();
            }
        }
        finally
        {
            if (sqlTransaction.Connection != null && sqlTransaction.Connection.State == ConnectionState.Open)
                sqlTransaction.Connection.Close();
        }
    }
    public void RollbackTransaction(NpgsqlTransaction sqlTransaction)
    {
        try
        {
            if (sqlTransaction.Connection != null)
            {
                using var conn = sqlTransaction.Connection;
                sqlTransaction.Rollback();
                conn.Close();
            }
        }
        finally
        {
            if (sqlTransaction.Connection != null && sqlTransaction.Connection.State == ConnectionState.Open)
                sqlTransaction.Connection.Close();
        }
    }
}
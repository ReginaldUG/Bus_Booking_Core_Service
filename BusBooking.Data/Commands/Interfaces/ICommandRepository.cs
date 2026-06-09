using Npgsql;

namespace BusBooking.Data.Commands.Interfaces;

public interface ICommandRepository<TEntity> where TEntity : class
{
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task<int> AddWithOpenDBTransaction(TEntity entity, NpgsqlTransaction sqltransaction);
    NpgsqlTransaction BeginTransaction();
    void CommitTransaction(NpgsqlTransaction sqlTransaction);
    void RollbackTransaction(NpgsqlTransaction sqlTransaction);

}
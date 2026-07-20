using Npgsql;

namespace BusBooking.Data.Commands.Interfaces;

public interface ICommandRepository<TEntity> where TEntity : class
{
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity, string propertyName, string value);
    Task<TEntity> AddWithOpenDBTransaction(TEntity entity, NpgsqlTransaction sqltransaction);
    Task UpdateWithOpenDbTransactionAsync(TEntity entity, NpgsqlTransaction sqltransaction);
    NpgsqlTransaction BeginTransaction();
    void CommitTransaction(NpgsqlTransaction sqlTransaction);
    void RollbackTransaction(NpgsqlTransaction sqlTransaction);

}
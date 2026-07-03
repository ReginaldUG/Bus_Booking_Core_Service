namespace BusBooking.Data.Helpers.Interfaces;

public interface IReadUtilities
{
    string GetConnectionString();
    string GenerateSelectCountInAsync<TEntity>(KeyValuePair<string, List<string>> criteria);
    string GenerateSelectQuery<TEntity>(int pageSize, int pageNumber);
    string GenerateSelectSingleRecordQuery<TEntity>(string propertyName, string value);
    string GenerateSelectMultipleRecordsQuery<TEntity>(string propertyName, string? value);
    string GenerateSelectLimitedRecordsQuery<TEntity>(string propertyName, string? value, int limit);
    string GenerateSelectByMultipleValuesListQuery<TEntity>(string propertyName, IEnumerable<string> values);
    string GenerateSelectByMultipleFieldsQuery<TEntity>(Dictionary<string, object> criteria, int? queryLimit);

}
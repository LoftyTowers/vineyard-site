using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using LayeredMicroservice.Application;
using LayeredMicroservice.Domain;
using LayeredMicroservice.Shared;
using Microsoft.Extensions.Logging;

namespace LayeredMicroservice.Infrastructure;

public sealed class SqlOrderRepository : IOrderRepository
{
    private readonly DbConnection _connection;
    private readonly ILogger<SqlOrderRepository> _logger;

    public SqlOrderRepository(DbConnection connection, ILogger<SqlOrderRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<Result> SaveAsync(Order order, CancellationToken cancellationToken)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "insert into orders (id, customer_id, total) values (@id, @customerId, @total)";
            command.AddParameter("@id", order.Id);
            command.AddParameter("@customerId", order.CustomerId);
            command.AddParameter("@total", order.Total);
            await command.ExecuteNonQueryAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbException ex) when (ex.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Constraint violation when persisting order {OrderId}", order.Id);
            return Result.Failure(ErrorCode.Domain, new[] { "Duplicate order" });
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "Database failure when writing order {OrderId}", order.Id);
            return Result.Failure(ErrorCode.Unexpected, new[] { "Database failure" });
        }
    }
}

public static class DbCommandExtensions
{
    public static void AddParameter(this DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}

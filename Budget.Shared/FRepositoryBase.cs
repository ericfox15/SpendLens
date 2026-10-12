using Microsoft.Data.SqlClient;

namespace Budget.Shared;

/// <summary>
/// Base class for all repositories. Holds the connection string and provides GetConnection().
/// Mirrors eMAR's FRepositoryBase — the only job is creating SqlConnection objects.
/// Each repository method opens and closes its own connection (not shared state).
/// </summary>
public abstract class FRepositoryBase(string connectionString)
{
    protected SqlConnection GetConnection() => new(connectionString);
}

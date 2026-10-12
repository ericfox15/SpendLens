using DbUp;
using Microsoft.Extensions.Configuration;
using System.Reflection;

// Budget.Database — DbUp migration runner.
// Run: dotnet run --project Budget.Database
// Or:  dotnet run --project Budget.Database -- --connection "Server=...;Database=BudgetHelper;..."
//
// Scripts in DeploymentScript/ run once (tracked in SchemaVersions table).
// Scripts in RunAlways/ run every deploy (views, stored procedures).

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

var connectionString = config.GetConnectionString("BudgetDatabase")
    ?? config["connection"]
    ?? throw new InvalidOperationException(
        "Provide a connection string via ConnectionStrings:BudgetDatabase in appsettings.json, " +
        "the CONNECTIONSTRINGS__BUDGETDATABASE env var, or the --connection CLI arg.");

Console.WriteLine($"Running migrations on: {connectionString[..Math.Min(60, connectionString.Length)]}...");

EnsureDatabase.For.SqlDatabase(connectionString);

// One-time deployment scripts (tracked by SchemaVersions)
var deployUpgrader = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(),
        s => s.Contains("DeploymentScript"))
    .LogToConsole()
    .Build();

var deployResult = deployUpgrader.PerformUpgrade();
if (!deployResult.Successful)
{
    Console.Error.WriteLine($"Deployment script failed: {deployResult.Error}");
    return 1;
}

// RunAlways scripts (views, stored procedures — re-run every deploy)
var runAlwaysUpgrader = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(),
        s => s.Contains("RunAlways"))
    // RunAlways scripts use a custom table name so they re-run every deploy
    .JournalToSqlTable("dbo", "SchemaVersionsRunAlways")
    .LogToConsole()
    .Build();

var runAlwaysResult = runAlwaysUpgrader.PerformUpgrade();
if (!runAlwaysResult.Successful)
{
    Console.Error.WriteLine($"RunAlways script failed: {runAlwaysResult.Error}");
    return 1;
}

Console.WriteLine("All migrations completed successfully.");
return 0;

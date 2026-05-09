using System.Text.Json;
using DipcServer.Models;
using Microsoft.Data.Sqlite;

namespace DipcServer.Data;

public sealed class ReportStore
{
    private readonly string _dbPath;

    public ReportStore(string dbPath)
    {
        _dbPath = dbPath;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var dir = Path.GetDirectoryName(_dbPath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var cmd = connection.CreateCommand();
        cmd.CommandText = """
CREATE TABLE IF NOT EXISTS reports (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  received_at_utc TEXT NOT NULL,
  machine_id TEXT NOT NULL,
  computer_name TEXT NULL,
  json TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_reports_machine_id ON reports(machine_id);
""";
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertAsync(PcReport report, DateTimeOffset receivedAtUtc, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = false });

        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var cmd = connection.CreateCommand();
        cmd.CommandText = """
INSERT INTO reports (received_at_utc, machine_id, computer_name, json)
VALUES ($received_at_utc, $machine_id, $computer_name, $json);
""";
        cmd.Parameters.AddWithValue("$received_at_utc", receivedAtUtc.UtcDateTime.ToString("O"));
        cmd.Parameters.AddWithValue("$machine_id", report.MachineId);
        cmd.Parameters.AddWithValue("$computer_name", report.ComputerName);
        cmd.Parameters.AddWithValue("$json", json);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MachineSummary>> ListMachinesAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var cmd = connection.CreateCommand();
        cmd.CommandText = """
SELECT machine_id,
       computer_name,
       MAX(received_at_utc) AS last_received_at_utc
FROM reports
GROUP BY machine_id
ORDER BY last_received_at_utc DESC;
""";

        var list = new List<MachineSummary>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var machineId = reader.GetString(0);
            var computerName = reader.IsDBNull(1) ? null : reader.GetString(1);
            var lastReceived = reader.GetString(2);

            list.Add(new MachineSummary
            {
                MachineId = machineId,
                ComputerName = computerName,
                LastReceivedAtUtc = DateTimeOffset.Parse(lastReceived)
            });
        }

        return list;
    }

    public async Task<StoredReport?> GetLatestAsync(string machineId, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var cmd = connection.CreateCommand();
        cmd.CommandText = """
SELECT received_at_utc, json
FROM reports
WHERE machine_id = $machine_id
ORDER BY received_at_utc DESC
LIMIT 1;
""";
        cmd.Parameters.AddWithValue("$machine_id", machineId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var received = DateTimeOffset.Parse(reader.GetString(0));
        var json = reader.GetString(1);
        return new StoredReport { MachineId = machineId, ReceivedAtUtc = received, Json = json };
    }
}

public sealed class MachineSummary
{
    public string MachineId { get; init; } = "";
    public string? ComputerName { get; init; }
    public DateTimeOffset LastReceivedAtUtc { get; init; }
}

public sealed class StoredReport
{
    public string MachineId { get; init; } = "";
    public DateTimeOffset ReceivedAtUtc { get; init; }
    public string Json { get; init; } = "";
}


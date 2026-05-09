using System.Diagnostics.Eventing.Reader;

namespace DipcClient;

public static class WindowsEventLogCollector
{
    public static List<EventItem> GetCriticalErrorWarningEvents(TimeSpan lookBack, int maxItems)
    {
        var items = new List<EventItem>();

        try
        {
            var start = DateTimeOffset.UtcNow - lookBack;
            items.AddRange(ReadLog("System", start, maxItems));
            items.AddRange(ReadLog("Application", start, maxItems));
        }
        catch
        {
        }

        return items
            .OrderByDescending(e => e.TimeCreatedUtc ?? DateTimeOffset.MinValue)
            .Take(maxItems)
            .ToList();
    }

    private static IEnumerable<EventItem> ReadLog(string logName, DateTimeOffset startUtc, int maxItems)
    {
        var list = new List<EventItem>();
        try
        {
            var query = $"*[System[(Level=1 or Level=2 or Level=3) and TimeCreated[@SystemTime>='{startUtc.UtcDateTime:O}']]]";
            var eventQuery = new EventLogQuery(logName, PathType.LogName, query)
            {
                ReverseDirection = true
            };

            using var reader = new EventLogReader(eventQuery);

            for (var i = 0; i < maxItems; i++)
            {
                using var record = reader.ReadEvent();
                if (record is null)
                {
                    break;
                }

                string? message = null;
                try
                {
                    message = record.FormatDescription();
                }
                catch
                {
                }

                list.Add(new EventItem
                {
                    TimeCreatedUtc = record.TimeCreated is null ? null : new DateTimeOffset(DateTime.SpecifyKind(record.TimeCreated.Value, DateTimeKind.Local)).ToUniversalTime(),
                    LogName = logName,
                    Level = record.LevelDisplayName,
                    LevelNumber = record.Level is null ? null : record.Level.Value,
                    Provider = record.ProviderName,
                    EventId = record.Id,
                    Message = message
                });
            }
        }
        catch
        {
        }

        return list;
    }
}

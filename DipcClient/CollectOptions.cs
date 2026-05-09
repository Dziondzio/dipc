namespace DipcClient;

public sealed class CollectOptions
{
    public bool CollectEvents { get; init; } = true;
    public int EventLookbackDays { get; init; } = 7;
    public int MaxEvents { get; init; } = 200;

    public bool CollectTemperatures { get; init; } = true;
    public bool CollectSmartDiskInfo { get; init; } = true;
}


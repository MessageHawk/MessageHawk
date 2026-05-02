namespace MessageHawk.Application.Sharding;

public static class InterchangeSharding
{
    /// <summary>Stable shard index in <c>[0, shardCount)</c> for routing to a single FIFO queue.</summary>
    public static int GetShardIndex(Guid interchangeId, int shardCount)
    {
        if (shardCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(shardCount));

        Span<byte> bytes = stackalloc byte[16];
        interchangeId.TryWriteBytes(bytes);

        const uint fnvOffset = 2166136261;
        const uint fnvPrime = 16777619;
        var hash = fnvOffset;
        for (var i = 0; i < 16; i++)
        {
            hash ^= bytes[i];
            hash *= fnvPrime;
        }

        return (int)(hash % (uint)shardCount);
    }
}

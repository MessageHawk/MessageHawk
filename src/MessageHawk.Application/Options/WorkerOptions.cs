namespace MessageHawk.Application.Options;

public sealed class WorkerOptions
{
    public const string SectionName = "Worker";

    /// <summary>
    /// When set, this process only consumes that shard (for Kubernetes: one pod per shard).
    /// When null, consumes all shards in-process (suitable for local dev and small installs).
    /// </summary>
    public int? AssignedShardIndex { get; set; }
}

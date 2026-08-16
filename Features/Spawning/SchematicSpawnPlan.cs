using ProjectMER.Features.Serializable.Schematics;

namespace DynamicSchematicOptimizer.Features.Spawning;

internal sealed class SchematicSpawnPlan
{
    internal SchematicSpawnPlan(int sourceBlockCount, List<SchematicBlockData> serverSideBlocks,
        IReadOnlyList<PlannedClientBlock> clientSideBlocks)
    {
        SourceBlockCount = sourceBlockCount;
        ServerSideBlocks = serverSideBlocks;
        ClientSideBlocks = clientSideBlocks;
    }

    internal int SourceBlockCount { get; }

    internal List<SchematicBlockData> ServerSideBlocks { get; }

    internal IReadOnlyList<PlannedClientBlock> ClientSideBlocks { get; }
}
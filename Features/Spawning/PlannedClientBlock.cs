using ProjectMER.Features.Serializable.Schematics;

namespace DynamicSchematicOptimizer.Features.Spawning;

internal readonly struct PlannedClientBlock
{
    internal PlannedClientBlock(SchematicBlockData block, bool parentIsClientSide)
    {
        Block = block;
        ParentIsClientSide = parentIsClientSide;
    }

    internal SchematicBlockData Block { get; }

    internal bool ParentIsClientSide { get; }
}
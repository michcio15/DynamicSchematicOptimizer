using AdminToys;

using ProjectMER.Features.Enums;
using ProjectMER.Features.Serializable.Schematics;

namespace DynamicSchematicOptimizer.Features.Spawning;

internal static class ClientSideSchematicPlanner
{
    private static readonly Dictionary<string, SchematicSpawnPlan> ByName = new();

    internal static SchematicSpawnPlan GetOrBuild(string schematicName, SchematicObjectDataList data,
        SchematicOptimisationConfig config)
    {
        if (ByName.TryGetValue(schematicName, out SchematicSpawnPlan? cached)
            && cached.SourceBlockCount == data.Blocks.Count)
        {
            return cached;
        }

        SchematicSpawnPlan plan = Build(schematicName, data, config);
        ByName[schematicName] = plan;
        return plan;
    }


    internal static void Clear()
    {
        ByName.Clear();
    }

    private static SchematicSpawnPlan Build(string schematicName, SchematicObjectDataList data,
        SchematicOptimisationConfig config)
    {
        List<SchematicBlockData> serverSideBlocks = new();
        Dictionary<int, SchematicBlockData> optimizedById = new();

        foreach (SchematicBlockData block in data.Blocks)
        {
            if (ShouldBeOptimized(block, config))
            {
                optimizedById[block.ObjectId] = block;
            }
            else
            {
                serverSideBlocks.Add(block);
            }
        }

        List<PlannedClientBlock> ordered = new(optimizedById.Count);
        HashSet<int> planned = new();
        HashSet<int> inProgress = new();

        // Iterate over the original list so siblings keep the order from the file.
        foreach (SchematicBlockData block in data.Blocks)
        {
            if (optimizedById.ContainsKey(block.ObjectId))
            {
                Plan(block, schematicName, optimizedById, planned, inProgress, ordered);
            }
        }

        WarnOnDroppedSubtrees(schematicName, serverSideBlocks, optimizedById);

        Log.Debug($"Built plan for schematic {schematicName} | Client sided : {ordered.Count} | Server sided : {serverSideBlocks.Count}");
        return new SchematicSpawnPlan(data.Blocks.Count, serverSideBlocks, ordered);
    }

    private static void Plan(SchematicBlockData block, string schematicName,
        Dictionary<int, SchematicBlockData> optimizedById, HashSet<int> planned, HashSet<int> inProgress,
        List<PlannedClientBlock> ordered)
    {
        if (!planned.Add(block.ObjectId))
        {
            return;
        }

        bool parentIsClientSide = false;

        if (optimizedById.TryGetValue(block.ParentId, out SchematicBlockData? parentBlock))
        {
            if (inProgress.Contains(block.ParentId))
            {
                Log.Warn(
                    $"Cycle in the hierarchy of schematic {schematicName}: block {block.ObjectId} is its own ancestor - parenting to {block.ParentId} skipped");
            }
            else
            {
                inProgress.Add(block.ObjectId);
                Plan(parentBlock, schematicName, optimizedById, planned, inProgress, ordered);
                inProgress.Remove(block.ObjectId);
                parentIsClientSide = true;
            }
        }

        ordered.Add(new PlannedClientBlock(block, parentIsClientSide));
    }

    private static void WarnOnDroppedSubtrees(string schematicName, IEnumerable<SchematicBlockData> serverSideBlocks,
        Dictionary<int, SchematicBlockData> optimizedById)
    {
        int orphaned = serverSideBlocks.Count(block => optimizedById.ContainsKey(block.ParentId));

        if (orphaned == 0)
        {
            return;
        }

        Log.Warn(
            $"Schematic {schematicName}: {orphaned} server-side block(s) have an optimized parent - ProjectMER will skip them together with their subtrees");
    }

    private static bool ShouldBeOptimized(SchematicBlockData blockData, SchematicOptimisationConfig optimisationConfig)
    {
        if (optimisationConfig.Primitive.Enabled && blockData.BlockType == BlockType.Primitive)
        {
            if (blockData.Properties.TryGetValue("PrimitiveFlags", out object flags) && optimisationConfig.Primitive.DontOptimizeWithCollision &&
                HasCollision(flags))
            {
                return false;
            }

            return true;
        }

        if (optimisationConfig.TextToy.Enabled && blockData.BlockType == BlockType.Text)
        {
            return true;
        }

        if (optimisationConfig.LightSource.Enabled && blockData.BlockType == BlockType.Light)
        {
            return true;
        }

        return false;

        static bool HasCollision(object flags)
        {
            PrimitiveFlags primitiveFlags = (PrimitiveFlags)Convert.ToByte(flags);
            return primitiveFlags.HasFlag(PrimitiveFlags.Collidable);
        }
    }
}
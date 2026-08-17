using MEROptimizer;

using ProjectMER.Features.Serializable.Schematics;

namespace DynamicSchematicOptimizer.Compatibility;

public class MerOptimizerCompatibility
{
    private readonly object _merOptimizer;

    public MerOptimizerCompatibility()
    {
        _merOptimizer = Plugin.merOptimizer;
    }

    public bool ShouldSchematicBeOptimized(SerializableSchematic serializableSchematic)
    {
        return ShouldSchematicBeOptimized(serializableSchematic.SchematicName);
    }

    public bool ShouldSchematicBeOptimized(string name)
    {
        if (DynamicSchematicOptimizerPlugin.Config.ExcludedMEROSchematics.Any(s => s == name))
        {
            return false;
        }


        MEROptimizer.Application.MEROptimizer merOptimizer = (MEROptimizer.Application.MEROptimizer)_merOptimizer;
        return merOptimizer.excludedNames.Any(n => name.ToLowerInvariant().Contains(n));
    }
}
using MEROptimizer;

using ProjectMER.Features.Serializable.Schematics;

namespace DynamicSchematicOptimizer.Compatibility;

public class MerOptimizerCompatibility
{
    private readonly MEROptimizer.Application.MEROptimizer _merOptimizer;

    public MerOptimizerCompatibility()
    {
        _merOptimizer = Plugin.merOptimizer;
    }

    public bool ShouldSchematicBeOptimized(SerializableSchematic serializableSchematic)
    {
        if (DynamicSchematicOptimizerPlugin.Config.ExcludedMEROSchematics.Any(s => s == serializableSchematic.SchematicName))
        {
            return false;
        }

        return _merOptimizer.excludedNames.Any(n => serializableSchematic.SchematicName.ToLowerInvariant().Contains(n));
    }
}
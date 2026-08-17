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
        return ShouldSchematicBeOptimized(serializableSchematic.SchematicName);
    }

    public bool ShouldSchematicBeOptimized(string name)
    {
        if (DynamicSchematicOptimizerPlugin.Config.ExcludedMEROSchematics.Any(s => s == name))
        {
            return false;
        }

        return _merOptimizer.excludedNames.Any(n => name.ToLowerInvariant().Contains(n));
    }
}
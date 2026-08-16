using AdminToys;

using JetBrains.Annotations;

using ProjectMER.Features.Serializable.Schematics;

namespace DynamicSchematicOptimizer.Events;

/// <summary>
/// Dispatches Optimizer-specific schematic lifecycle events.
/// </summary>
[PublicAPI]
public static class SchematicEventHandler
{
    /// <summary>
    /// Invoked right after ProjectMER decided whether a <see cref="SerializableSchematic"/> may spawn.
    /// Unlike ProjectMER's own event, this exposes the <see cref="SerializableSchematic"/> instance itself.
    /// </summary>
    public static event Action<SchematicSpawningExtendedEventArgs>? SchematicSpawning;

    /// <summary>
    /// Raises the <see cref="SchematicSpawning"/> event.
    /// </summary>
    /// <param name="ev">The event arguments.</param>
    public static void OnSchematicSpawning(SchematicSpawningExtendedEventArgs ev)
    {
        SchematicSpawning?.Invoke(ev);
    }
}

public class SchematicSpawningExtendedEventArgs
{
    public SchematicSpawningExtendedEventArgs(SerializableSchematic schematic, SchematicObjectDataList data,
        PrimitiveObjectToy toy, bool overrideDefault)
    {
        Schematic = schematic;
        Data = data;
        Toy = toy;
        OverrideDefault = overrideDefault;
    }

    public SerializableSchematic Schematic { get; }

    public SchematicObjectDataList Data { get; }

    public PrimitiveObjectToy Toy { get; }

    /// <summary>
    /// Czy bedziemy zmieniali system spawnowania
    /// </summary>
    public bool OverrideDefault { get; set; }
}
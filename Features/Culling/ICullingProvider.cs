using JetBrains.Annotations;

using LabApi.Features.Wrappers;

namespace DynamicSchematicOptimizer.Features.Culling;

/// <summary>
/// Represents a provider of <see cref="ICullable"/> objects.
/// </summary>
[PublicAPI]
public interface ICullingProvider
{
    /// <summary>
    /// Called every <see cref="Config.CullingTickTimeInBetween"/> seconds to check if should be culled.
    /// </summary>
    void Tick();

    /// <summary>
    /// Shows the bounds of the culling provider.
    /// </summary>
    void ShowDebugBounds();

    /// <summary>
    /// The <see cref="Player"/>s that should be ignored by the culling provider."/>
    /// </summary>
    HashSet<Player> Ignored { get; }

    /// <summary>
    /// Represents a collection of <see cref="Player"/> objects that have been marked as spawned
    /// by the culling provider.
    /// </summary>
    HashSet<Player> Spawned { get; }
}
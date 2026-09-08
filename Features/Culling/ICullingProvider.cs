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
    /// The <see cref="Player"/>s that should be ignored by the culling provider."/>
    /// </summary>
    HashSet<Player> Ignored { get; }

    /// <summary>
    /// A collection of <see cref="Player"/>s that have spawned this culling provider.
    /// </summary>
    HashSet<Player> Spawned { get; }

    /// <summary>
    /// Called every <see cref="Config.CullingTickTimeInBetween"/> seconds to check if should be culled.
    /// </summary>
    void Tick();

    /// <summary>
    /// Shows the bounds of the culling provider.
    /// </summary>
    void ShowDebugBounds();
}
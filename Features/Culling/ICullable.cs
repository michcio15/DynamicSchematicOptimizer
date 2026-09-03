using LabApi.Features.Wrappers;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features.Culling;

/// <summary>
/// Represents an object that can be culled.
/// </summary>
public interface ICullable
{
    /// <summary>
    /// Spawns the object for the <paramref name="player"/>.
    /// </summary>
    /// <param name="player"><see cref="Player"/> for whom it will be spawned.</param>
    void Spawn(Player player);

    /// <summary>
    /// Destroys the object for the <paramref name="player"/>.
    /// </summary>
    /// <param name="player"><see cref="Player"/> for whom it will be destroyed.</param>
    void Destroy(Player player);

    /// <summary>
    /// Gets the world position of the object.
    /// </summary>
    /// <returns><see cref="Vector3"/> of the position</returns>
    Vector3 GetWorldPosition();
}
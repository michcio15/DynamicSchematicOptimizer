using DynamicSchematicOptimizer.Features.Toys;

using JetBrains.Annotations;

using LabApi.Features.Wrappers;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features.Culling;

[PublicAPI]
public abstract class BaseCullingProvider : ICullingProvider
{
    protected BaseCullingProvider(ICullable cullable)
    {
        Cullable = cullable;
    }

    public virtual void Tick()
    {
        Vector3 pos = GetWorldPosition();
        foreach (Player player in Player.ReadyList)
        {
            if (Ignored.Contains(player))
            {
                continue;
            }

            bool spawned = Spawned.Contains(player);
            bool isVisible = IsVisible(player, pos);


            if (!isVisible && spawned)
            {
                Cullable.Destroy(player);
                continue;
            }

            if (isVisible && !spawned)
            {
                Cullable.Spawn(player);
            }
        }
    }

    public virtual void ShowDebugBounds()
    {
    }

    /// <summary>
    /// The <see cref="ICullable"/> that will be culled.
    /// </summary>
    public ICullable Cullable { get; }

    public virtual HashSet<Player> Ignored { get; } = new();
    public virtual HashSet<Player> Spawned { get; } = new();

    /// <summary>
    /// Should the <see cref="Cullable"/> be visible for the <paramref name="player"/> at <paramref name="position"/>.
    /// </summary>
    /// <param name="player"><see cref="Player"/> for whom we will check the visibility.</param>
    /// <param name="position">The position of the object.</param>
    /// <returns>Either should be visible or not.</returns>
    public abstract bool IsVisible(Player player, Vector3 position);

    /// <summary>
    /// Get world position of the cullable.
    /// </summary>
    /// <returns>World position.</returns>
    /// <remarks>Does not work on <see cref="ClientSideAdminToy"/> with parents without overloading</remarks>
    protected virtual Vector3 GetWorldPosition()
    {
        return Cullable.GetWorldPosition();
    }
}
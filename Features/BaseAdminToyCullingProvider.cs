using DynamicSchematicOptimizer.Features.Toys;

using JetBrains.Annotations;

using LabApi.Features.Wrappers;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features;

[PublicAPI]
public abstract class BaseAdminToyCullingProvider : ICullingProvider
{
    protected BaseAdminToyCullingProvider(ClientSideAdminToy clientSideAdminToy)
    {
        ClientSideAdminToy = clientSideAdminToy;
    }

    public ClientSideAdminToy ClientSideAdminToy { get; }

    public HashSet<Player> Ignored { get; } = new();
    public HashSet<Player> Spawned { get; } = new();

    public void Tick()
    {
        Vector3 pos = ClientSideAdminToy.Position;
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
                ClientSideAdminToy.Destroy(player.Connection);
                continue;
            }

            if (isVisible && !spawned)
            {
                ClientSideAdminToy.Spawn(player.Connection);
            }
        }
    }

    public void ShowDebugBounds()
    {
    }

    public abstract bool IsVisible(Player player, Vector3 position);
}
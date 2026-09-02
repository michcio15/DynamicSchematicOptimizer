using DrawableLine;

using MapGeneration;

using PlayerRoles;

using UnityEngine;

using Player = LabApi.Features.Wrappers.Player;

namespace DynamicSchematicOptimizer.Features;

public class BoundsCulling : ICullingProvider
{
    public BoundsCulling(ClientSidedSchematic clientSidedSchematic)
    {
        ClientSidedSchematic = clientSidedSchematic;
        Radius = ClientSidedSchematic.OptimisationConfig.CullingDistance;
    }

    public ClientSidedSchematic ClientSidedSchematic { get; }
    public byte Radius { get; }

    public Vector3 Position => ClientSidedSchematic.SchematicObject.Position;

    public void Tick()
    {
        Vector3 pos = ClientSidedSchematic.SchematicObject.Position;
        foreach (Player player in Player.ReadyList)
        {
            if (ClientSidedSchematic.Ignored.Contains(player))
            {
                continue;
            }

            bool spawned = ClientSidedSchematic.Spawned.Contains(player);
            bool isVisible = IsVisible(player, pos);


            if (!isVisible && spawned)
            {
                ClientSidedSchematic.Destroy(player);
                continue;
            }

            if (isVisible && !spawned)
            {
                ClientSidedSchematic.Spawn(player);
            }
        }
    }

    public bool IsVisible(Player player, Vector3 position)
    {
        if (!player.Role.IsAlive() || player.Role == RoleTypeId.Scp079 || player.Zone == FacilityZone.Surface)
        {
            return true;
        }

        if (Radius <= 0)
        {
            return true;
        }

        if (position == Vector3.zero)
        {
            return true;
        }

        if ((position - player.Position).sqrMagnitude <= Radius * Radius)
        {
            return true;
        }

        return false;
    }

    public void ShowDebugBounds()
    {
        Vector3 position = Position;
        Bounds bounds = new(position, Vector3.one * Radius * 2);
        bool isDebug = DrawableLines.IsDebugModeEnabled;
        DrawableLines.IsDebugModeEnabled = true;
        DrawableLines.GenerateSphere(position, 1, 10, Color.red);
        DrawableLines.GenerateSphere(position, Radius, Color.green);
        DrawableLines.IsDebugModeEnabled = isDebug;
    }
}
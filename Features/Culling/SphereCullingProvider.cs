using DrawableLine;

using JetBrains.Annotations;

using LabApi.Features.Wrappers;

using MapGeneration;

using PlayerRoles;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features.Culling;

[PublicAPI]
public class SphereCullingProvider : BaseCullingProvider
{
    public SphereCullingProvider(ICullable cullable) : base(cullable)
    {
    }

    public virtual byte Radius { get; set; } = 33;

    public override bool IsVisible(Player player, Vector3 position)
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

    public override void ShowDebugBounds()
    {
        Vector3 position = GetWorldPosition();
        DrawableLines.GenerateSphere(position, 1, 10, Color.red);
        DrawableLines.GenerateSphere(position, Radius, 10, Color.green, 10);
    }
}
using Mirror;

using ProjectMER.Features;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features.Toys;

public class ClientSideLightSourceToy : ClientSideAdminToy
{
    public float LightIntensity { get; set; }
    public float LightRange { get; set; }
    public Color LightColor { get; set; }
    public LightShadows ShadowType { get; set; }
    public float ShadowStrength { get; set; }
    public LightType LightType { get; set; }
#pragma warning disable CS0618 // Type or member is obsolete
    public LightShape LightShape { get; set; }
#pragma warning restore CS0618 // Type or member is obsolete
    public float SpotAngle { get; set; }
    public float InnerSpotAngle { get; set; }
    protected override uint AssetID { get; } = PrefabManager.LightSource.netIdentity.assetId;

    protected override void WriteSyncVars(NetworkWriter writer)
    {
        base.WriteSyncVars(writer);
        writer.WriteFloat(LightIntensity);
        writer.WriteFloat(LightRange);
        writer.WriteColor(LightColor);
        writer.Write(ShadowType);
        writer.WriteFloat(ShadowStrength);
        writer.Write(LightType);
        writer.Write(LightShape);
        writer.WriteFloat(SpotAngle);
        writer.WriteFloat(InnerSpotAngle);
    }
}
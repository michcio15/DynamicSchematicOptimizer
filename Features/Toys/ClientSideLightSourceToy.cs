using Mirror;

using ProjectMER.Features;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features.Toys;

public class ClientSideLightSourceToy : ClientSideAdminToy
{
    public float LightIntensity
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(32UL);
        }
    }

    public float LightRange
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(64UL);
        }
    }

    public Color LightColor
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(128UL);
        }
    }

    public LightShadows ShadowType
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(256UL);
        }
    }

    public float ShadowStrength
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(512UL);
        }
    }

    public LightType LightType
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(1024UL);
        }
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public LightShape LightShape
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(2048UL);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete

    public float SpotAngle
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(4096UL);
        }
    }

    public float InnerSpotAngle
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(8192UL);
        }
    }

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

    protected override void WriteSyncVarsDelta(NetworkWriter writer)
    {
        base.WriteSyncVarsDelta(writer);

        writer.WriteULong(DirtyBits);

        if ((DirtyBits & 32UL) != 0)
        {
            writer.WriteFloat(LightIntensity);
        }

        if ((DirtyBits & 64UL) != 0)
        {
            writer.WriteFloat(LightRange);
        }

        if ((DirtyBits & 128UL) != 0)
        {
            writer.WriteColor(LightColor);
        }

        if ((DirtyBits & 256UL) != 0)
        {
            writer.Write(ShadowType);
        }

        if ((DirtyBits & 512UL) != 0)
        {
            writer.WriteFloat(ShadowStrength);
        }

        if ((DirtyBits & 1024UL) != 0)
        {
            writer.Write(LightType);
        }

        if ((DirtyBits & 2048UL) != 0)
        {
            writer.Write(LightShape);
        }

        if ((DirtyBits & 4096UL) != 0)
        {
            writer.WriteFloat(SpotAngle);
        }

        if ((DirtyBits & 8192UL) != 0)
        {
            writer.WriteFloat(InnerSpotAngle);
        }
    }
}
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
            DirtyBits |= 32UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    }

    public float LightRange
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 64UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    }

    public Color LightColor
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 128UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    }

    public LightShadows ShadowType
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 256UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    }

    public float ShadowStrength
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 512UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    }

    public LightType LightType
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 1024UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public LightShape LightShape
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 2048UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete

    public float SpotAngle
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 4096UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    }

    public float InnerSpotAngle
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 8192UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
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
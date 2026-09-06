using Mirror;

using ProjectMER.Features;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features.Toys;

public class ClientSideLightSourceToy : ClientSideAdminToy
{
    /// <summary>
    /// Gets or sets the intensity of the light.
    /// </summary>
    public float LightIntensity
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(32UL);
        }
    }

    /// <summary>
    /// Gets or sets the range of the light.
    /// </summary>
    public float LightRange
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(64UL);
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="Color"/> of the light.
    /// </summary>
    public Color LightColor
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(128UL);
        }
    }

    /// <summary>
    /// Gets or sets the type of shadows.
    /// </summary>
    public LightShadows ShadowType
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(256UL);
        }
    }

    /// <summary>
    /// Gets or sets the strength of the shadows.
    /// </summary>
    public float ShadowStrength
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(512UL);
        }
    }

    /// <summary>
    /// Gets or sets the type of light.
    /// </summary>
    public LightType LightType
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(1024UL);
        }
    }

    /// <summary>
    /// Gets or sets the shape of the light.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the spot angle of the light.
    /// </summary>
    public float SpotAngle
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(4096UL);
        }
    }

    /// <summary>
    /// Gets or sets the inner spot angle of the light.
    /// </summary>
    public float InnerSpotAngle
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(8192UL);
        }
    }

    protected override uint AssetID => PrefabManager.LightSource.netIdentity.assetId;

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
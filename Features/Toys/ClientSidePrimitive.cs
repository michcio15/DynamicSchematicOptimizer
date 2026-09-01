using AdminToys;

using Mirror;

using ProjectMER.Features;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features.Toys;

public class ClientSidePrimitive : ClientSideAdminToy
{
    public PrimitiveType Type
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

    public Color Color
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

    public PrimitiveFlags Flags
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

    protected override uint AssetID { get; } = PrefabManager.PrimitiveObject.netIdentity.assetId;

    protected override void WriteSyncVars(NetworkWriter writer)
    {
        base.WriteSyncVars(writer);
        writer.Write(Type);
        writer.WriteColor(Color);
        writer.Write(Flags);
    }

    protected override void WriteSyncVarsDelta(NetworkWriter writer)
    {
        base.WriteSyncVarsDelta(writer);

        writer.WriteULong(DirtyBits);

        if ((DirtyBits & 32UL) != 0)
        {
            writer.Write(Type);
        }

        if ((DirtyBits & 64UL) != 0)
        {
            writer.WriteColor(Color);
        }

        if ((DirtyBits & 128UL) != 0)
        {
            writer.Write(Flags);
        }
    }
}
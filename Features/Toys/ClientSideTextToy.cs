using Mirror;

using ProjectMER.Features;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features.Toys;

public class ClientSideTextToy : ClientSideAdminToy
{
    public Vector2 Size
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

    public string TextFormat
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 64UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    } = string.Empty;

    protected override uint AssetID { get; } = PrefabManager.Text.netIdentity.assetId;

    protected override void WriteSyncVars(NetworkWriter writer)
    {
        base.WriteSyncVars(writer);
        writer.WriteVector2(Size);
        writer.WriteString(TextFormat);
    }

    protected override void WriteSyncObjects(NetworkWriter writer)
    {
        base.WriteSyncObjects(writer);
        writer.WriteUInt(0);
        writer.WriteUInt(0);
    }

    protected override void WriteSyncVarsDelta(NetworkWriter writer)
    {
        base.WriteSyncVarsDelta(writer);

        writer.WriteULong(DirtyBits);

        if ((DirtyBits & 32UL) != 0)
        {
            writer.Write(Size);
        }

        if ((DirtyBits & 64UL) != 0)
        {
            writer.WriteString(TextFormat);
        }
    }
}
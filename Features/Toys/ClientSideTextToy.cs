using AdminToys;

using Mirror;

using ProjectMER.Features;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features.Toys;

public class ClientSideTextToy : ClientSideAdminToy
{
    public Vector2 DisplaySize
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 32UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    } = TextToy.DefaultDisplaySize;

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
    } = "Please write smth here <3";

    protected override uint AssetID { get; } = PrefabManager.Text.netIdentity.assetId;

    protected override void WriteSyncVars(NetworkWriter writer)
    {
        base.WriteSyncVars(writer);
        writer.WriteVector2(DisplaySize);
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
        // I would like to send my love to the jebana kurwa synclist.
        writer.WriteULong(0);

        base.WriteSyncVarsDelta(writer);

        writer.WriteULong(DirtyBits);

        if ((DirtyBits & 32UL) != 0)
        {
            writer.WriteVector2(DisplaySize);
        }

        if ((DirtyBits & 64UL) != 0)
        {
            writer.WriteString(TextFormat);
        }
    }
}
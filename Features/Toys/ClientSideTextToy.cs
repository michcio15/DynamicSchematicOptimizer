using Mirror;

using ProjectMER.Features;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features.Toys;

public class ClientSideTextToy : ClientSideAdminToy
{
    public Vector2 Size { get; set; }
    public string TextFormat { get; set; } = string.Empty;
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
}
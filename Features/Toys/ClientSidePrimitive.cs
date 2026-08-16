using AdminToys;

using Mirror;

using ProjectMER.Features;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features.Toys;

public class ClientSidePrimitive : ClientSideAdminToy
{
    public PrimitiveType Type { get; set; }
    public Color Color { get; set; }
    public PrimitiveFlags Flags { get; set; }
    protected override uint AssetID { get; } = PrefabManager.PrimitiveObject.netIdentity.assetId;

    protected override void WriteSyncVars(NetworkWriter writer)
    {
        base.WriteSyncVars(writer);
        writer.Write(Type);
        writer.WriteColor(Color);
        writer.Write(Flags);
    }
}
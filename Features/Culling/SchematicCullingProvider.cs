using Player = LabApi.Features.Wrappers.Player;

namespace DynamicSchematicOptimizer.Features.Culling;

public class SchematicCullingProvider : SphereCullingProvider
{
    public SchematicCullingProvider(ClientSidedSchematic clientSidedSchematic) : base(clientSidedSchematic)
    {
    }

    public ClientSidedSchematic ClientSidedSchematic => (ClientSidedSchematic)Cullable;

    public override HashSet<Player> Ignored => ClientSidedSchematic.Ignored;
    public override HashSet<Player> Spawned => ClientSidedSchematic.Spawned;

    public override byte Radius => ClientSidedSchematic.OptimisationConfig.CullingDistance;
}
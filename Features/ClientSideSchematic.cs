using DynamicSchematicOptimizer.Features.Toys;

using JetBrains.Annotations;

using ProjectMER.Features.Objects;

using Player = LabApi.Features.Wrappers.Player;

namespace DynamicSchematicOptimizer.Features;

[PublicAPI]
public class ClientSidedSchematic
{
    public ClientSidedSchematic(uint netID, List<ClientSideAdminToy> toys, SchematicObject schematicObject, SchematicOptimisationConfig optimisationConfig)
    {
        NetID = netID;
        Toys = toys;
        SchematicObject = schematicObject;
        OptimisationConfig = optimisationConfig;
        BoundsCulling = new BoundsCulling(this);
    }

    public HashSet<Player> Spawned { get; set; } = new();
    public HashSet<Player> Ignored { get; set; } = new();

    public List<ClientSideAdminToy> Toys { get; }

    public SchematicOptimisationConfig OptimisationConfig { get; }
    public uint NetID { get; set; }
    public BoundsCulling BoundsCulling { get; private set; }
    public SchematicObject SchematicObject { get; }

    public void SpawnForAll()
    {
        foreach (Player player in Player.ReadyList)
        {
            Spawn(player);
        }
    }

    public void Spawn(Player player)
    {
        Toys.ForEach(toy => toy.Spawn(player.Connection));
        Spawned.Add(player);
        Log.Debug($"Spawning {SchematicObject.name} for {player.Nickname}");
    }

    public void Destroy(Player player)
    {
        for (int i = Toys.Count - 1; i >= 0; i--)
        {
            Toys[i].Destroy(player.Connection);
        }

        Spawned.Remove(player);
        Log.Debug($"Destroying {SchematicObject.name} for {player.Nickname}");
    }

    public void Destroy()
    {
        for (int i = Toys.Count - 1; i >= 0; i--)
        {
            Toys[i].DestroyForAll();
        }

        Spawned.Clear();
    }
}
using DynamicSchematicOptimizer.Features.Toys;

using JetBrains.Annotations;

using Mirror;

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

    /// <summary>
    /// Should <see cref="Player"/> be ignored with <see cref="BoundsCulling"/>
    /// </summary>
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
        if (Spawned.Contains(player))
        {
            Log.Warn($"Player {player.Nickname} already spawned {SchematicObject.name}!");
            return;
        }

        Toys.ForEach(toy => toy.Spawn(player.Connection));
        Spawned.Add(player);
        Log.Debug($"Spawning {SchematicObject.name} for {player.Nickname}");
    }

    /// <summary>
    /// Destroys all of the <see cref="Toys"/> for the player, server-sided toys stay.
    /// </summary>
    /// <param name="player"><see cref="Player"/> for whom the toys will be destroyed</param>
    public void Destroy(Player player)
    {
        for (int i = Toys.Count - 1; i >= 0; i--)
        {
            Toys[i].Destroy(player.Connection);
        }

        Spawned.Remove(player);
        Log.Debug($"Destroying {SchematicObject.name} for {player.Nickname}");
    }

    /// <summary>
    /// Destroys entire schematic for the player, including server-sided toys.
    /// </summary>
    /// <param name="player">The player that will schematic be destoryed for</param>
    /// <param name="addToIgnored">If <see langword="true"/> then player will be added to <see cref="Ignored"/> so culling won't spawn it.</param>
    /// <remarks>This action cannot be undone only use this if you have, for example, a custom model for a player, and you don't want him to see it.</remarks>
    public void DestroyWithSchematic(Player player, bool addToIgnored = true)
    {
        Destroy(player);

        foreach (NetworkIdentity identity in SchematicObject.NetworkIdentities)
        {
            player.Connection.Send(new ObjectDestroyMessage
            {
                netId = identity.netId,
            });
        }

        player.Connection.Send(new ObjectDestroyMessage
        {
            netId = NetID,
        });

        if (addToIgnored)
        {
            Ignored.Add(player);
        }
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
using DynamicSchematicOptimizer.Features.Culling;

using JetBrains.Annotations;

using Mirror;

using UnityEngine;

using Player = LabApi.Features.Wrappers.Player;

namespace DynamicSchematicOptimizer.Features.Toys;

public abstract class ClientSideAdminToy : ICullable
{
    private const int RpcChangeParentHashCode = -342419096;

    protected ulong DirtyBits = 0UL;

    protected SpawnMessage? CachedSpawnMessage;

    protected EntityStateMessage? CachedEntityStateMessage;

    private bool _parentDirty = false;

    /// <inheritdoc />
    public void Spawn(Player player)
    {
        player.Connection.Send(GetSpawnMessage());

        CullingProvider?.Spawned.Add(player);
    }

    /// <inheritdoc />
    public void Destroy(Player player)
    {
        player.Connection.Send(new ObjectDestroyMessage
        {
            netId = NetId,
        });

        CullingProvider?.Spawned.Remove(player);
    }

    /// <inheritdoc />
    public Vector3 GetWorldPosition()
    {
        return Position;
    }

    /// <summary>
    /// Gets the <see cref="ClientSidedSchematic"/> that owns this toy.
    /// </summary>
    public ClientSidedSchematic? Schematic { get; internal set; }

    /// <summary>
    /// Gets or sets the position of the toy relative to the <see cref="Transform"/> of the <see cref="ParentNetId"/>.
    /// </summary>
    public Vector3 Position
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(1UL);
        }
    } = Vector3.zero;

    /// <summary>
    /// Gets or sets the rotation of the toy relative to the <see cref="Transform"/> of the <see cref="ParentNetId"/>.
    /// </summary>
    public Quaternion Rotation
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(2UL);
        }
    } = Quaternion.identity;

    /// <summary>
    /// Gets or sets the scale of the toy relative to the <see cref="Transform"/> of the <see cref="ParentNetId"/>.
    /// </summary>
    public Vector3 Scale
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(4UL);
        }
    } = Vector3.one;

    /// <summary>
    /// Gets or sets the movement smoothing of the toy.
    /// </summary>
    public byte MovementSmoothing
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(8UL);
        }
    } = 60;

    /// <summary>
    /// Gets or sets if the toy is static.
    /// </summary>
    public bool IsStatic
    {
        get;

        set
        {
            field = value;
            MarkDirtyBits(16UL);
        }
    } = false;

    /// <summary>
    /// Gets or sets the <see cref="NetworkIdentity.netId"/> of the <see cref="Transform"/> that the toy will be parented to.
    /// </summary>
    public uint ParentNetId
    {
        get;

        set
        {
            field = value;
            _parentDirty = true;
            CachedSpawnMessage = null;
        }
    } = 0;

    /// <summary>
    /// Gets the <see cref="NetworkIdentity.netId"/> of the toy. It does not exist on the server.
    /// </summary>
    public uint NetId
    {
        get
        {
            if (field == 0)
            {
                field = NetworkIdentity.GetNextNetworkId();
            }

            return field;
        }
    } = 0;

    /// <summary>
    /// Gets or sets the <see cref="ICullingProvider"/> that will be used to determine if the toy should be culled.
    /// Useful when you have a <see cref="AdminToys.TextToy"/> thats large.
    /// </summary>
    /// <remarks>Needs to be added to <see cref="SchematicSync.CullingProviders"/> in order to work.</remarks>
    [PublicAPI]
    public ICullingProvider? CullingProvider { get; set; } = null;

    /// <summary>
    /// The prefabs asset id.
    /// </summary>
    protected abstract uint AssetID { get; }

    /// <summary>
    /// Sets the parent of the toy. The <paramref name="parent"/> needs to have a <see cref="NetworkIdentity"/> component.
    /// </summary>
    /// <param name="parent"><see cref="Transform"/> which will be the parent</param>
    /// <returns>Itself</returns>
    public ClientSideAdminToy SetParent(Transform parent)
    {
        if (parent.TryGetComponent(out NetworkIdentity networkIdentity))
        {
            ParentNetId = networkIdentity.netId;
        }
        else
        {
            Log.Warn($"{parent.name} does not have a {nameof(NetworkIdentity)}");
        }

        return this;
    }

    /// <summary>
    /// Spawns the toy for all players.
    /// </summary>
    public void SpawnForAll()
    {
        SpawnMessage spawnMessage = GetSpawnMessage();

        foreach (Player p in Player.ReadyList)
        {
            p.Connection.Send(spawnMessage);
            CullingProvider?.Spawned.Add(p);
        }
    }

    /// <summary>
    /// Destroys the toy for all players.
    /// </summary>
    public void DestroyForAll()
    {
        ObjectDestroyMessage msg = new()
        {
            netId = NetId,
        };
        foreach (Player p in Player.ReadyList)
        {
            p.Connection.Send(msg);
        }

        CullingProvider?.Spawned.Clear();
    }

    /// <summary>
    /// Syncs the toy to all players.
    /// </summary>
    public void Sync()
    {
        EntityStateMessage entityStateMessage = GetEntityStateMessage();
        foreach (Player p in Player.ReadyList)
        {
            if (_parentDirty)
            {
                SendChangeParent(p.Connection);
            }

            p.Connection.Send(entityStateMessage);
        }

        _parentDirty = false;
    }

    /// <summary>
    /// Gets the <see cref="SpawnMessage"/> that will be sent to the player.
    /// </summary>
    /// <returns>The <see cref="SpawnMessage"/></returns>
    public SpawnMessage GetSpawnMessage()
    {
        if (CachedSpawnMessage.HasValue)
        {
            return CachedSpawnMessage.Value;
        }

        using NetworkWriterPooled writer = NetworkWriterPool.Get();

        Compression.CompressVarUInt(writer, 1UL);

        int headerPos = writer.Position;
        writer.WriteByte(0);
        int contentPos = writer.Position;
        WriteSyncObjects(writer);
        WriteSyncVars(writer);
        WriteOnSerialize(writer);
        int endPos = writer.Position;
        writer.Position = headerPos;
        writer.WriteByte((byte)((endPos - contentPos) & 0xFF));
        writer.Position = endPos;

        CachedSpawnMessage = new SpawnMessage
        {
            assetId = AssetID,
            position = Position,
            rotation = Rotation,
            scale = Scale,
            netId = NetId,
            payload = new ArraySegment<byte>(writer.ToArray()),
        };
        return CachedSpawnMessage.Value;
    }

    /// <summary>
    /// Gets the <see cref="EntityStateMessage"/> that will be sent to the player.
    /// </summary>
    /// <returns>The <see cref="EntityStateMessage"/></returns>
    public EntityStateMessage GetEntityStateMessage()
    {
        if (CachedEntityStateMessage.HasValue)
        {
            return CachedEntityStateMessage.Value;
        }

        using NetworkWriterPooled writer = NetworkWriterPool.Get();

        Compression.CompressVarUInt(writer, 1UL);

        int headerPos = writer.Position;
        writer.WriteByte(0);
        int contentPos = writer.Position;

        WriteSyncObjectsDelta(writer);
        WriteSyncVarsDelta(writer);

        int endPos = writer.Position;
        writer.Position = headerPos;
        writer.WriteByte((byte)((endPos - contentPos) & 0xFF));
        writer.Position = endPos;

        CachedEntityStateMessage = new EntityStateMessage
        {
            netId = NetId,
            payload = new ArraySegment<byte>(writer.ToArray()),
        };

        DirtyBits = 0UL;
        return CachedEntityStateMessage.Value;
    }

    protected virtual void WriteOnSerialize(NetworkWriter writer)
    {
        writer.Write(ParentNetId);
    }

    protected virtual void WriteSyncVars(NetworkWriter writer)
    {
        writer.WriteVector3(Position);
        writer.WriteQuaternion(Rotation);
        writer.WriteVector3(Scale);
        writer.WriteByte(MovementSmoothing);
        writer.WriteBool(IsStatic);
    }

    protected virtual void WriteSyncObjects(NetworkWriter writer)
    {
    }

    /// <summary>
    /// Writes the sync object dirty mask that Mirror emits at the start of every non-initial payload
    /// (see <see cref="NetworkBehaviour.SerializeObjectsDelta"/>). Admin toys own no sync objects, so it is always 0.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    protected virtual void WriteSyncObjectsDelta(NetworkWriter writer)
    {
        writer.WriteULong(0UL);
    }

    /// <summary>
    /// Marks the given syncvar <paramref name="dirtyBits"/> as changed and invalidates the cached messages.
    /// </summary>
    /// <param name="dirtyBits">The syncvar dity bits to mark in <see cref="DirtyBits"/>.</param>
    protected void MarkDirtyBits(ulong dirtyBits)
    {
        DirtyBits |= dirtyBits;
        CachedEntityStateMessage = null;
        CachedSpawnMessage = null;
    }

    protected virtual void WriteSyncVarsDelta(NetworkWriter writer)
    {
        writer.WriteULong(DirtyBits);

        if ((DirtyBits & 1UL) != 0)
        {
            writer.WriteVector3(Position);
        }

        if ((DirtyBits & 2UL) != 0)
        {
            writer.WriteQuaternion(Rotation);
        }

        if ((DirtyBits & 4UL) != 0)
        {
            writer.WriteVector3(Scale);
        }

        if ((DirtyBits & 8UL) != 0)
        {
            writer.WriteByte(MovementSmoothing);
        }

        if ((DirtyBits & 16UL) != 0)
        {
            writer.WriteBool(IsStatic);
        }
    }

    private void SendChangeParent(NetworkConnection connection)
    {
        using NetworkWriterPooled writer = NetworkWriterPool.Get();
        writer.WriteUInt(ParentNetId);
        connection.Send(new RpcMessage
        {
            netId = NetId,
            componentIndex = 0,
            functionHash = unchecked((ushort)RpcChangeParentHashCode),
            payload = writer.ToArraySegment(),
        });
    }
}
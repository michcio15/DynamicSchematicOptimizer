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

    public void Spawn(Player player)
    {
        player.Connection.Send(GetSpawnMessage());

        CullingProvider?.Spawned.Add(player);
    }

    public void Destroy(Player player)
    {
        player.Connection.Send(new ObjectDestroyMessage
        {
            netId = NetId,
        });

        CullingProvider?.Spawned.Remove(player);
    }

    public Vector3 GetWorldPosition()
    {
        if (ParentNetId == 0UL)
        {
            return Position;
        }

        Log.Warn("I ");
        return Position;
    }

    //private bool _parentDirty = false;

    public Vector3 Position
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 1UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    } = Vector3.zero;

    public Quaternion Rotation
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 2UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    } = Quaternion.identity;

    public Vector3 Scale
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 4UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    } = Vector3.one;

    public byte MovementSmoothing
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 8UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    } = 60;

    public bool IsStatic
    {
        get;

        set
        {
            field = value;
            DirtyBits |= 16UL;
            CachedEntityStateMessage = null;
            CachedSpawnMessage = null;
        }
    } = false;

    public uint ParentNetId
    {
        get;

        set
        {
            field = value;
            //_parentDirty = true;
            CachedSpawnMessage = null;
        }
    } = 0;

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

    protected abstract uint AssetID { get; }

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

    public void SpawnForAll()
    {
        SpawnMessage spawnMessage = GetSpawnMessage();

        foreach (Player p in Player.ReadyList)
        {
            p.Connection.Send(spawnMessage);
            CullingProvider?.Spawned.Add(p);
        }
    }

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

    public void Sync()
    {
        EntityStateMessage entityStateMessage = GetEntityStateMessage();
        foreach (Player p in Player.ReadyList)
        {
            p.Connection.Send(entityStateMessage);
        }
    }


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
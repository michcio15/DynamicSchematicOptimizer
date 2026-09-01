using Mirror;

using UnityEngine;

using Player = LabApi.Features.Wrappers.Player;

namespace DynamicSchematicOptimizer.Features.Toys;

public abstract class ClientSideAdminToy
{
    private const int RpcChangeParentHashCode = -342419096;

    protected ulong DirtyBits = 0UL;

    protected SpawnMessage? CachedSpawnMessage;

    protected EntityStateMessage? CachedEntityStateMessage;

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
    }

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
    }

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
    }

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
    }

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
    }

    public uint ParentNetId
    {
        get;

        set
        {
            field = value;
            //_parentDirty = true;
            CachedSpawnMessage = null;
        }
    }

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

    public void Spawn(NetworkConnection conn)
    {
        conn.Send(GetSpawnMessage());
    }

    public void SpawnForAll()
    {
        SpawnMessage spawnMessage = GetSpawnMessage();

        foreach (Player p in Player.ReadyList)
        {
            p.Connection.Send(spawnMessage);
        }
    }

    public void Destroy(NetworkConnection conn)
    {
        conn.Send(new ObjectDestroyMessage
        {
            netId = NetId,
        });
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
    }

    public void Sync(NetworkConnection conn)
    {
        EntityStateMessage entityStateMessage = GetEntityStateMessage();
        conn.Send(entityStateMessage);
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

        writer.WriteULong(0UL);

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
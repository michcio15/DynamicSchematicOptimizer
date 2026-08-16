using Mirror;

using UnityEngine;

using Player = LabApi.Features.Wrappers.Player;

namespace DynamicSchematicOptimizer.Features.Toys;

public abstract class ClientSideAdminToy
{
    private bool _spawnMessageSet = false;

    private SpawnMessage _spawnMessage;

    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Scale { get; set; }
    public byte MovementSmoothing { get; set; }
    public bool IsStatic { get; set; }

    public uint ParentNetId { get; set; }

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

    public SpawnMessage GetSpawnMessage(NetworkWriter writer)
    {
        if (_spawnMessageSet)
        {
            return _spawnMessage;
        }

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

        _spawnMessage = new SpawnMessage
        {
            assetId = AssetID,
            position = Position,
            rotation = Rotation,
            scale = Scale,
            netId = NetId,
            payload = new ArraySegment<byte>(writer.ToArray()),
        };
        _spawnMessageSet = true;
        return _spawnMessage;
    }

    public void Spawn(NetworkConnection conn)
    {
        using NetworkWriterPooled writer = NetworkWriterPool.Get();
        conn.Send(GetSpawnMessage(writer));
    }

    public void SpawnForAll()
    {
        using NetworkWriterPooled writer = NetworkWriterPool.Get();
        SpawnMessage spawnMessage = GetSpawnMessage(writer);

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
}
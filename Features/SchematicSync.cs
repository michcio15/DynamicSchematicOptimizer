using System.Diagnostics.CodeAnalysis;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using MEC;

namespace DynamicSchematicOptimizer.Features;

public static class SchematicSync
{
    internal static readonly Dictionary<uint, ClientSidedSchematic> ByNetID = new();

    private static float _timeBetweenTicks = 0;

    private static CoroutineHandle? _coroutineHandle = null;

    public static IReadOnlyCollection<ClientSidedSchematic> Schematics => ByNetID.Values;

    public static bool TryGetSchematic(uint netID, [NotNullWhen(true)] out ClientSidedSchematic? schematic)
    {
        return ByNetID.TryGetValue(netID, out schematic);
    }

    public static bool TryDestroySchematic(uint netID)
    {
        if (!ByNetID.TryGetValue(netID, out ClientSidedSchematic? schematic))
        {
            return false;
        }

        schematic.Destroy();

        ByNetID.Remove(netID);
        if (ByNetID.Count == 0 && _coroutineHandle != null)
        {
            Timing.KillCoroutines(_coroutineHandle.Value);
            _coroutineHandle = null;
            Log.Debug("Killed culling coroutine");
        }

        Log.Debug($"Destroying schematic {schematic.SchematicObject.name}");
        return true;
    }

    public static void AddSchematic(ClientSidedSchematic schematic)
    {
        ByNetID.Add(schematic.NetID, schematic);
        _coroutineHandle ??= Timing.RunCoroutine(SpawningCoroutine());
    }

    internal static void Register()
    {
        ByNetID.Clear();
        _timeBetweenTicks = DynamicSchematicOptimizerPlugin.Config.CullingTickTimeInBetween;
        PlayerEvents.Left += OnLeft;
    }

    internal static void Unregister()
    {
        ByNetID.Clear();
        PlayerEvents.Left -= OnLeft;
    }

    private static void OnLeft(PlayerLeftEventArgs ev)
    {
        foreach (ClientSidedSchematic clientSided in ByNetID.Values)
        {
            clientSided.Spawned.Remove(ev.Player);
        }
    }

    /*private static void OnJoined(PlayerJoinedEventArgs ev)
    {
        foreach (ClientSidedSchematic schematic in ByNetID.Values)
        {
            schematic.Spawn(ev.Player);
        }
    }*/

    private static IEnumerator<float> SpawningCoroutine()
    {
        while (ByNetID.Count > 0)
        {
            foreach (ClientSidedSchematic schematic in ByNetID.Values)
            {
                schematic.BoundsCulling.Tick();
            }

            yield return Timing.WaitForSeconds(_timeBetweenTicks);
        }

        _coroutineHandle = null;
    }
}
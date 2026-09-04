using System.Diagnostics.CodeAnalysis;

using DynamicSchematicOptimizer.Features.Culling;
using DynamicSchematicOptimizer.Features.Toys;

using JetBrains.Annotations;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;

using MEC;

namespace DynamicSchematicOptimizer.Features;

/// <summary>
/// Handles the synchronization of <see cref="ClientSidedSchematic"/> and also culling.
/// </summary>
[PublicAPI]
public static class SchematicSync
{
    /// <summary>
    /// If you have a custom <see cref="ICullingProvider"/> per toy you should add it here.
    /// </summary>
    [PublicAPI]
    public static readonly List<ICullingProvider> CullingProviders = new();

    internal static readonly Dictionary<uint, ClientSidedSchematic> ByNetID = new();

    private static float _timeBetweenTicks = 0;

    private static CoroutineHandle? _coroutineHandle = null;

    /// <summary>
    /// All <see cref="ClientSidedSchematic"/>s that are currently spawned.
    /// </summary>
    public static IReadOnlyCollection<ClientSidedSchematic> Schematics => ByNetID.Values;

    /// <summary>
    /// Attempts to retrieve the <see cref="ClientSidedSchematic"/> associated with the specified <paramref name="netID"/>.
    /// </summary>
    /// <param name="netID">The network ID of the schematic to retrieve.</param>
    /// <param name="schematic">The retrieved <see cref="ClientSidedSchematic"/> if successfully found; otherwise, null.</param>
    /// <returns><see langword="true"/> if the schematic was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetSchematic(uint netID, [NotNullWhen(true)] out ClientSidedSchematic? schematic)
    {
        return ByNetID.TryGetValue(netID, out schematic);
    }

    /// <summary>
    /// Attempts to destroy the <see cref="ClientSidedSchematic"/> associated with the specified <paramref name="netID"/>.
    /// </summary>
    /// <param name="netID">The network ID of the schematic to destroy.</param>
    /// <returns><see langword="true"/> if the schematic was successfully destroyed; otherwise, <see langword="false"/>.</returns>
    public static bool TryDestroySchematic(uint netID)
    {
        if (!ByNetID.TryGetValue(netID, out ClientSidedSchematic? schematic))
        {
            return false;
        }

        schematic.Destroy();

        CullingProviders.Remove(schematic.SchematicCullingProvider);
        foreach (ClientSideAdminToy toy in schematic.Toys)
        {
            if (toy.CullingProvider != null)
            {
                CullingProviders.Remove(toy.CullingProvider);
            }
        }

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

    /// <summary>
    /// Adds <paramref name="schematic"/> to the <see cref="ByNetID"/> and <see cref="CullingProviders"/>.
    /// </summary>
    /// <param name="schematic"><see cref="ClientSidedSchematic"/> that will be added</param>
    public static void AddSchematic(ClientSidedSchematic schematic)
    {
        ByNetID.Add(schematic.NetID, schematic);
        CullingProviders.Add(schematic.SchematicCullingProvider);
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
        CullingProviders.Clear();

        if (_coroutineHandle != null)
        {
            Timing.KillCoroutines(_coroutineHandle.Value);
            _coroutineHandle = null;
        }


        PlayerEvents.Left -= OnLeft;
    }

    private static void OnLeft(PlayerLeftEventArgs ev)
    {
        Player player = ev.Player;

        foreach (ICullingProvider provider in CullingProviders)
        {
            provider.Ignored.Remove(player);
            provider.Spawned.Remove(player);
        }
    }

    private static IEnumerator<float> SpawningCoroutine()
    {
        while (CullingProviders.Count > 0)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < CullingProviders.Count; i++)
            {
                ICullingProvider cullingProvider = CullingProviders[i];
                cullingProvider.Tick();
            }

            yield return Timing.WaitForSeconds(_timeBetweenTicks);
        }

        _coroutineHandle = null;
    }
}
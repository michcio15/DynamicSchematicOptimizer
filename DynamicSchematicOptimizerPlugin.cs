using DynamicSchematicOptimizer.Events;
using DynamicSchematicOptimizer.Features;
using DynamicSchematicOptimizer.Features.Spawning;
using DynamicSchematicOptimizer.Features.Toys;

using HarmonyLib;

using JetBrains.Annotations;

using LabApi.Features;
using LabApi.Loader.Features.Plugins;

using Mirror;

using ProjectMER.Events.Arguments;
using ProjectMER.Events.Handlers;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable.Schematics;

using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace DynamicSchematicOptimizer;

[PublicAPI]
public class DynamicSchematicOptimizerPlugin : Plugin<Config>
{
    private static Harmony _harmony = null!;

    public static new Config Config { get; private set; } = null!;
    public static DynamicSchematicOptimizerPlugin Instance { get; private set; } = null!;

    public override string Name { get; } = "Dynamic Schematic Optimizer";
    public override string Description { get; } = "Plugin for ProjectMER that optimizes schematics ";
    public override string Author { get; } = "michcio";
    public override Version Version { get; } = new(1, 0, 0);
    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

    public override void Enable()
    {
        Instance = this;
        Config = base.Config;

        _harmony = new Harmony(nameof(DynamicSchematicOptimizerPlugin));
        _harmony.PatchAll();

        ConfigLoader.Init();
        SchematicSync.Register();

        SchematicEventHandler.SchematicSpawning += OnSchematicSpawning;
        Schematic.SchematicDestroyed += OnSchematicDestroyed;
    }

    public override void Disable()
    {
        Instance = null!;
        Config = null!;

        _harmony.UnpatchAll(_harmony.Id);

        ConfigLoader.Disable();
        SchematicSync.Unregister();

        SchematicEventHandler.SchematicSpawning -= OnSchematicSpawning;
        Schematic.SchematicDestroyed -= OnSchematicDestroyed;
    }

    private void OnSchematicSpawning(SchematicSpawningExtendedEventArgs ev)
    {
        if (!ConfigLoader.TryGetConfig(ev.Schematic.SchematicName, out SchematicOptimisationConfig? optimisationConfig) || !optimisationConfig.Enabled)
        {
            return;
        }

        SchematicSpawnPlan plan =
            ClientSideSchematicPlanner.GetOrBuild(ev.Schematic.SchematicName, ev.Data, optimisationConfig);

        if (plan.ClientSideBlocks.Count == 0)
        {
            return;
        }

        ev.OverrideDefault = true;

        PrimitiveObjectToy toy = ev.Toy;

        SchematicObject schematicObject = toy.gameObject.AddComponent<SchematicObject>();
        NetworkServer.Spawn(toy.gameObject);

        SchematicObjectDataList schematicObjectDataList = new()
        {
            Blocks = new List<SchematicBlockData>(plan.ServerSideBlocks),
            Path = ev.Data.Path,
            RootObjectId = ev.Data.RootObjectId,
        };
        schematicObject.Init(schematicObjectDataList);

        uint netId = schematicObject.gameObject.GetComponent<NetworkIdentity>().netId;
        List<ClientSideAdminToy> clientToys = ClientSideSchematicBuilder.Build(plan, schematicObject, optimisationConfig);

        ClientSidedSchematic clientSidedSchematic = new(netId, clientToys, schematicObject, optimisationConfig);
        clientSidedSchematic.SpawnForAll();
        SchematicSync.AddSchematic(clientSidedSchematic);
    }

    private void OnSchematicDestroyed(SchematicDestroyedEventArgs ev)
    {
        uint netId = ev.Schematic.gameObject.GetComponent<NetworkIdentity>().netId;
        SchematicSync.TryDestroySchematic(netId);
    }
}
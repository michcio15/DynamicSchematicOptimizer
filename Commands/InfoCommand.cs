using System.Diagnostics.CodeAnalysis;
using System.Text;

using CommandSystem;

using DynamicSchematicOptimizer.Features;
using DynamicSchematicOptimizer.Features.Toys;

using Mirror;

using NorthwoodLib.Pools;

using ProjectMER.Features;
using ProjectMER.Features.Objects;

using UnityEngine;

namespace DynamicSchematicOptimizer.Commands;

public class InfoCommand : BaseOptimizerCommand, IUsageProvider
{
    public string[] Usage { get; } = ["optional schematic name"];

    public override string Command { get; } = "info";
    public override string[] Aliases { get; } = ["i"];
    public override string Description { get; } = "Information about optimized schematics";
    protected override string RequiredPermission => DynamicSchematicOptimizerPlugin.Config.Permissions.Info;

    private static string BuildReport(string schematicName, ClientSidedSchematic sidedSchematic)
    {
        Dictionary<string, int> serverSideByType = sidedSchematic.SchematicObject.AdminToyBases
            .GroupBy(static toy => toy.GetType().Name)
            .ToDictionary(static g => g.Key, static g => g.Count());

        Dictionary<string, int> clientSideByType = sidedSchematic.Toys
            .GroupBy(GetFriendlyTypeName)
            .ToDictionary(static g => g.Key, static g => g.Count());

        int serverSideCount = serverSideByType.Values.Sum();
        int clientSideCount = clientSideByType.Values.Sum();
        int total = serverSideCount + clientSideCount;

        StringBuilder sb = StringBuilderPool.Shared.Rent();
        sb.AppendLine($"Schematic '{schematicName}' | total objects: {total}");

        sb.AppendLine($"Server-side: {serverSideCount} ({Percent(serverSideCount, total)})");
        foreach (KeyValuePair<string, int> entry in serverSideByType.OrderByDescending(static kv => kv.Value))
        {
            sb.AppendLine($"  - {entry.Key}: {entry.Value} ({Percent(entry.Value, serverSideCount)})");
        }

        sb.AppendLine($"Client-side: {clientSideCount} ({Percent(clientSideCount, total)})");
        foreach (KeyValuePair<string, int> entry in clientSideByType.OrderByDescending(static kv => kv.Value))
        {
            sb.AppendLine($"  - {entry.Key}: {entry.Value} ({Percent(entry.Value, clientSideCount)})");
        }

        string report = StringBuilderPool.Shared.ToStringReturn(sb).Trim();
        return report;
    }

    private static string Percent(int part, int whole)
    {
        return whole == 0 ? "0%" : $"{part * 100f / whole:F1}%";
    }

    private static string GetFriendlyTypeName(ClientSideAdminToy toy)
    {
        return toy.GetType().Name;
    }

    public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!CheckPermission(sender, out response))
        {
            return false;
        }

        if (arguments.IsEmpty())
        {
            int clientSidedToys = 0;
            foreach (ClientSidedSchematic schematic in SchematicSync.ByNetID.Values)
            {
                clientSidedToys += schematic.Toys.Count;
            }

            int serverSidedToys = 0;
            foreach (ClientSidedSchematic schematic in SchematicSync.ByNetID.Values)
            {
                serverSidedToys += schematic.SchematicObject.AdminToyBases.Count;
            }

            response = $"Spawned schematics: {SchematicSync.ByNetID.Values.Count}\n" +
                       $"Client sided: {clientSidedToys} | Server sided: {serverSidedToys}\n";
            return true;
        }

        string schematicName = string.Join(" ", arguments);

        if (!ConfigLoader.TryGetConfig(schematicName, out SchematicOptimisationConfig? config))
        {
            if (DynamicSchematicOptimizerPlugin.Instance.MEROCompatibility == null ||
                !DynamicSchematicOptimizerPlugin.Instance.MEROCompatibility.ShouldSchematicBeOptimized(schematicName))
            {
                response = $"{schematicName} is not optimized";
                return false;
            }

            config = DynamicSchematicOptimizerPlugin.Config.DefaultConfig;
        }

        if (!config.Enabled)
        {
            response = $"{schematicName} has optimization disabled in the config";
            return false;
        }

        if (!ObjectSpawner.TrySpawnSchematic(schematicName, Vector3.zero, out SchematicObject schematicObject))
        {
            response = "No such schematic";
            return false;
        }

        uint netId = schematicObject.GetComponent<NetworkIdentity>().netId;

        if (!SchematicSync.TryGetSchematic(netId, out ClientSidedSchematic? sidedSchematic))
        {
            response = $"Failed to find {nameof(ClientSidedSchematic)} as optimized";
            schematicObject.Destroy();
            return false;
        }

        response = BuildReport(schematicName, sidedSchematic);
        schematicObject.Destroy();
        return true;
    }
}
using System.Diagnostics.CodeAnalysis;

using CommandSystem;

using DynamicSchematicOptimizer.Features;

using LabApi.Features.Wrappers;

namespace DynamicSchematicOptimizer.Commands;

public class CullingCommand : BaseOptimizerCommand, IUsageProvider
{
    private static readonly HashSet<string> ShowStrings = new() { "s", "show" };

    public string[] Usage { get; } = ["nothing / show"];

    public override string Command { get; } = "culling";
    public override string[] Aliases { get; } = ["cull", "cu", "cul"];
    public override string Description { get; } = "Information about culling";
    protected override string RequiredPermission => DynamicSchematicOptimizerPlugin.Config.Permissions.Culling;

    private static bool BuildCullingInfo(ICommandSender sender, out string response)
    {
        if (!Player.TryGet(sender, out Player? player))
        {
            response = "You must be a player";
            return false;
        }

        int hiddenSchematics = 0;
        int hiddenToys = 0;
        int visibleSchematics = 0;
        int visibleToys = 0;
        foreach (ClientSidedSchematic schematic in SchematicSync.ByNetID.Values)
        {
            int toysCount = GetToys(schematic);

            if (schematic.Spawned.Contains(player))
            {
                visibleSchematics++;
                visibleToys += toysCount;
            }
            else
            {
                hiddenSchematics++;
                hiddenToys += toysCount;
            }
        }

        response = $"Visible schematics: {visibleSchematics} ({visibleToys}) / Hidden: {hiddenSchematics} ({hiddenToys})";
        return true;
    }

    private static int GetToys(ClientSidedSchematic schematic)
    {
        int count = 0;
        count += schematic.Toys.Count;
        count += schematic.SchematicObject.AdminToyBases.Count;
        return count;
    }

    public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!CheckPermission(sender, out response))
        {
            return false;
        }

        if (arguments.IsEmpty())
        {
            return BuildCullingInfo(sender, out response);
        }

        string arg = arguments[0].ToLowerInvariant();

        if (!ShowStrings.Contains(arg))
        {
            response = $"Usage: {this.DisplayCommandUsage()}";
            return false;
        }

        foreach (ClientSidedSchematic clientSidedSchematic in SchematicSync.ByNetID.Values)
        {
            clientSidedSchematic.BoundsCulling.ShowDebugBounds();
        }

        response = "Culling bounds shown";
        return true;
    }
}
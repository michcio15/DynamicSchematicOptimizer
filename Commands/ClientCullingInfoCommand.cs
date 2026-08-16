using System.Diagnostics.CodeAnalysis;

using CommandSystem;

using DynamicSchematicOptimizer.Features;

using LabApi.Features.Wrappers;

namespace DynamicSchematicOptimizer.Commands;

[CommandHandler(typeof(ClientCommandHandler))]
public class ClientCullingInfoCommand : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
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
    }

    private static int GetToys(ClientSidedSchematic schematic)
    {
        int count = 0;
        count += schematic.Toys.Count;
        count += schematic.SchematicObject.AdminToyBases.Count;
        return count;
    }

    public string Command { get; } = "cullinginfo";
    public string[] Aliases { get; } = [];
    public string Description { get; } = "Information about schematic culling";
}
using System.Diagnostics.CodeAnalysis;

using CommandSystem;

namespace DynamicSchematicOptimizer.Commands;

public class CreateCommand : BaseOptimizerCommand
{
    public override string Command { get; } = "create";
    public override string[] Aliases { get; } = ["c"];
    public override string Description { get; } = "Creates new config";
    protected override string RequiredPermission => DynamicSchematicOptimizerPlugin.Config.Permissions.Create;

    public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!CheckPermission(sender, out response))
        {
            return false;
        }

        if (arguments.IsEmpty())
        {
            response = "You need to specify schematic name";
            return false;
        }

        string name = string.Join(" ", arguments);
        ConfigLoader.CreateConfig(name);
        response = $"Created optimization config for schematic: {name}";
        return true;
    }
}
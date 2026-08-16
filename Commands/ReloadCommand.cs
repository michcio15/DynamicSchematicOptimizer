using System.Diagnostics.CodeAnalysis;

using CommandSystem;

namespace DynamicSchematicOptimizer.Commands;

public class ReloadCommand : BaseOptimizerCommand
{
    public override string Command { get; } = "reload";
    public override string[] Aliases { get; } = ["r"];
    public override string Description { get; } = "Reloads the configs, needs to be called after schematics have been changed";
    protected override string RequiredPermission => DynamicSchematicOptimizerPlugin.Config.Permissions.Reload;

    public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!CheckPermission(sender, out response))
        {
            return false;
        }

        ConfigLoader.ReloadAll();
        response = "Successfully reloaded the configs";
        return true;
    }
}
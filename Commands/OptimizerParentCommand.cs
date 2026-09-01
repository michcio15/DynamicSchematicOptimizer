using System.Diagnostics.CodeAnalysis;

using CommandSystem;

namespace DynamicSchematicOptimizer.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
public class OptimizerParentCommand : ParentCommand
{
    public OptimizerParentCommand()
    {
        LoadGeneratedCommands();
    }

    public override string Command { get; } = "optimizer";
    public override string[] Aliases { get; } = [];
    public override string Description { get; } = "Optimizer command";

    public sealed override void LoadGeneratedCommands()
    {
        RegisterCommand(new ReloadCommand());
        RegisterCommand(new CreateCommand());
        RegisterCommand(new InfoCommand());
        RegisterCommand(new CullingCommand());
#if DEBUG
        RegisterCommand(new TestCommand());
#endif
    }

    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!CommandPermissions.Check(sender, DynamicSchematicOptimizerPlugin.Config.Permissions.Optimizer, out response))
        {
            return false;
        }

        response = AllCommands.Aggregate("Available commands:",
            static (current, command) => string.Concat(current, "\n", command.Command, " - ", command.Description));
        return false;
    }
}
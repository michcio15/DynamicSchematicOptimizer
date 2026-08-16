using System.Diagnostics.CodeAnalysis;

using CommandSystem;

namespace DynamicSchematicOptimizer.Commands;

public abstract class BaseOptimizerCommand : ICommand
{
    public abstract bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response);
    public abstract string Command { get; }
    public abstract string[] Aliases { get; }
    public abstract string Description { get; }
    protected abstract string RequiredPermission { get; }

    protected bool CheckPermission(ICommandSender sender, out string response)
    {
        return CommandPermissions.Check(sender, RequiredPermission, out response);
    }
}
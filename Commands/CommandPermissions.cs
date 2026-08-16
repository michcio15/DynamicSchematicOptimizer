using CommandSystem;

using LabApi.Features.Permissions;

namespace DynamicSchematicOptimizer.Commands;

/// <summary>
/// Shared permission gate for the optimizer commands. Each command supplies its permission node
/// from <see cref="CommandPermissionsConfig"/>; an empty node means the command is unrestricted.
/// </summary>
internal static class CommandPermissions
{
    internal static bool Check(ICommandSender sender, string permission, out string response)
    {
        if (string.IsNullOrWhiteSpace(permission) || sender.HasPermission(permission))
        {
            response = string.Empty;
            return true;
        }

        response = $"You don't have permission to use this command (missing '{permission}').";
        return false;
    }
}
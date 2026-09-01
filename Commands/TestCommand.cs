#if DEBUG
using System.Diagnostics.CodeAnalysis;

using AdminToys;

using CommandSystem;

using DynamicSchematicOptimizer.Features.Toys;

using LabApi.Features.Wrappers;

using MEC;

using UnityEngine;

namespace DynamicSchematicOptimizer.Commands;

public class TestCommand : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        Player player = Player.Get(sender)!;
        ClientSidePrimitive clientSidePrimitive = new()
        {
            Position = player.Position,
            Color = Color.blue,
            Type = PrimitiveType.Sphere,
            Flags = PrimitiveFlags.Visible,
            IsStatic = false,
            MovementSmoothing = 60,
            ParentNetId = 0,
            Rotation = player.Rotation,
            Scale = Vector3.one,
        };
        clientSidePrimitive.Spawn(player.Connection);
        Timing.CallDelayed(5, () =>
        {
            clientSidePrimitive.Color = Color.red;
            clientSidePrimitive.Rotation = player.Rotation;
            clientSidePrimitive.Sync(player.Connection);
        });
        response = string.Empty;
        return true;
    }

    public string Command { get; } = "test";
    public string[] Aliases { get; } = [];
    public string Description { get; } = "test";
}
#endif
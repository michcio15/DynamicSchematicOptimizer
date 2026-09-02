#if DEBUG

using System.Diagnostics.CodeAnalysis;

using CommandSystem;

using DynamicSchematicOptimizer.Features;
using DynamicSchematicOptimizer.Features.Culling;
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
        ClientSideTextToy textToy = new()
        {
            DisplaySize = new Vector2(50, 50),
            TextFormat = "Hello World",
            IsStatic = false,
            Position = player.Position,
            Rotation = player.Rotation,
        };
        textToy.CullingProvider = new SphereCullingProvider(textToy);
        //sphereCullingProvider.Spawned.Add(player);
        textToy.Spawn(player);
        SchematicSync.CullingProviders.Add(textToy.CullingProvider);
        /*Timing.CallDelayed(5, () =>
        {
            textToy.TextFormat = "Goodbye World";
            textToy.Position = player.Position + new Vector3(0, 1, 0);
            textToy.Sync();
        });*/
        response = string.Empty;
        return true;
    }

    public string Command { get; } = "test";
    public string[] Aliases { get; } = [];
    public string Description { get; } = "test";
}
#endif
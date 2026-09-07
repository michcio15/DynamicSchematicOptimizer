#if DEBUG

using System.Diagnostics.CodeAnalysis;

using AdminToys;

using CommandSystem;

using DynamicSchematicOptimizer.Extensions;
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
        ClientSideTextToy toy = new()
        {
            TextFormat = "cooo",
            IsStatic = false,
            Position = player.Position,
            Rotation = player.Rotation,
        };
        toy.AddCulling(new SphereCullingProvider(toy));
        Timing.CallDelayed(1f, () =>
        {
            toy.Position = Vector3.forward;
            toy.ParentNetId = player.NetworkId;
            toy.TextFormat = "oook";
            toy.Sync();
        });
        response = string.Empty;
        return true;
    }

    public string Command { get; } = "test";
    public string[] Aliases { get; } = [];
    public string Description { get; } = "test";
}
#endif
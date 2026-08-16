using System.Reflection;
using System.Reflection.Emit;

using AdminToys;

using DynamicSchematicOptimizer.Events;

using HarmonyLib;

using JetBrains.Annotations;

using ProjectMER.Features.Serializable.Schematics;

using UnityEngine;

namespace DynamicSchematicOptimizer.Patches;

[HarmonyPatch(typeof(SerializableSchematic), nameof(SerializableSchematic.SpawnOrUpdateObject))]
public static class SpawnOrUpdateObjectSchematicPatch
{
    [HarmonyTranspiler]
    [UsedImplicitly]
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        MethodInfo invoker =
            AccessTools.Method(typeof(SpawnOrUpdateObjectSchematicPatch), nameof(InvokeSchematicSpawning));
        MethodInfo getGameObject = AccessTools.PropertyGetter(typeof(Component), nameof(Component.gameObject));

        CodeMatcher matcher = new(instructions, generator);

        matcher.MatchEndForward(new CodeMatch(OpCodes.Ldnull),
            new CodeMatch(OpCodes.Ret),
            new CodeMatch(OpCodes.Ldloc_0));


        int toMove = matcher.Pos;
        matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldloc_3),
            new CodeInstruction(OpCodes.Ldloc_0),
            new CodeInstruction(OpCodes.Call, invoker));

        List<Label> labels = matcher.Labels;
        matcher.AddLabelsAt(toMove, labels);
        matcher.Labels.Clear();
        matcher.DefineLabel(out Label label).Labels.Add(label);
        //matcher.Advance(-1);
        matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label), new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Callvirt, getGameObject), new CodeInstruction(OpCodes.Ret))
            .ThrowIfInvalid("ododowo");


        return matcher.InstructionEnumeration();
    }

    private static bool InvokeSchematicSpawning(SerializableSchematic schematic,
        SchematicObjectDataList data, PrimitiveObjectToy toy)
    {
        //  Log.Error("dwaoidhwakdjhwalkdwaoi");
        SchematicSpawningExtendedEventArgs ev = new(schematic, data, toy, false);
        SchematicEventHandler.OnSchematicSpawning(ev);
        return ev.OverrideDefault;
    }
}
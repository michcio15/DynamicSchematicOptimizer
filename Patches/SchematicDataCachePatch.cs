using System.Diagnostics.CodeAnalysis;

using HarmonyLib;

using JetBrains.Annotations;

using ProjectMER.Features;
using ProjectMER.Features.Serializable.Schematics;

namespace DynamicSchematicOptimizer.Patches;

[HarmonyPatch(typeof(MapUtils), nameof(MapUtils.GetSchematicDataByName))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class SchematicDataCachePatch
{
    public static readonly Dictionary<string, SchematicObjectDataList> Cache = new();

    [UsedImplicitly]
    [HarmonyPrefix]
    private static bool Prefix(string schematicName, ref SchematicObjectDataList __result, out bool __state)
    {
        __state = Cache.TryGetValue(schematicName, out __result);
        return !__state;
    }

    [UsedImplicitly]
    [HarmonyPostfix]
    private static void Postfix(string schematicName, SchematicObjectDataList __result, bool __state)
    {
        if (!__state && __result != null)
        {
            Cache[schematicName] = __result;
        }
    }

    public static void Clear()
    {
        Cache.Clear();
    }
}
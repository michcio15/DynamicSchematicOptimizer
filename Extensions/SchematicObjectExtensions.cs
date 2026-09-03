using System.Diagnostics.CodeAnalysis;

using DynamicSchematicOptimizer.Features;

using JetBrains.Annotations;

using Mirror;

using ProjectMER.Features.Objects;

namespace DynamicSchematicOptimizer.Extensions;

[PublicAPI]
public static class SchematicObjectExtensions
{
    /// <summary>
    /// Checks if the schematic is optimized.
    /// </summary>
    /// <param name="schematicObject">The <see cref="SchematicObject"/> for which we check the optimization.</param>
    /// <param name="clientSidedSchematic">The optimized schematic. <see langword="null"/> if <see langword="false"/>.</param>
    /// <returns>Either is optimized or not.</returns>
    public static bool IsOptimized(this SchematicObject schematicObject, [NotNullWhen(true)] out ClientSidedSchematic? clientSidedSchematic)
    {
        uint netId = schematicObject.GetComponent<NetworkIdentity>().netId;
        return SchematicSync.ByNetID.TryGetValue(netId, out clientSidedSchematic);
    }
}
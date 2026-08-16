using System.Diagnostics.CodeAnalysis;

using DynamicSchematicOptimizer.Features;

using Mirror;

using ProjectMER.Features.Objects;

namespace DynamicSchematicOptimizer.Extensions;

public static class SchematicObjectExtensions
{
    extension(SchematicObject schematicObject)
    {
        public bool IsOptimized([NotNullWhen(true)] out ClientSidedSchematic? clientSidedSchematic)
        {
            uint netId = schematicObject.GetComponent<NetworkIdentity>().netId;
            return SchematicSync.ByNetID.TryGetValue(netId, out clientSidedSchematic);
        }
    }
}
using LabApi.Features.Wrappers;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features.Culling;

public interface ICullable
{
    void Spawn(Player player);

    void Destroy(Player player);

    Vector3 GetWorldPosition();
}
using JetBrains.Annotations;

using LabApi.Features.Wrappers;

namespace DynamicSchematicOptimizer.Features.Culling;

[PublicAPI]
public interface ICullingProvider
{
    void Tick();

    void ShowDebugBounds();

    HashSet<Player> Ignored { get; }
    HashSet<Player> Spawned { get; }
}
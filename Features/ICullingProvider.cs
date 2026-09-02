using JetBrains.Annotations;

namespace DynamicSchematicOptimizer.Features;

[PublicAPI]
public interface ICullingProvider
{
    void Tick();

    void ShowDebugBounds();
}
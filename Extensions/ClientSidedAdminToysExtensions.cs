using DynamicSchematicOptimizer.Features;
using DynamicSchematicOptimizer.Features.Culling;
using DynamicSchematicOptimizer.Features.Toys;

using JetBrains.Annotations;

namespace DynamicSchematicOptimizer.Extensions;

[PublicAPI]
public static class ClientSidedAdminToysExtensions
{
    extension(ClientSideAdminToy clientSideAdminToy)
    {
        /// <summary>
        /// Adds a culling provider to the toy.
        /// </summary>
        /// <param name="culling">The instance of the <see cref="ICullingProvider"/></param>
        /// <typeparam name="T"><see cref="ICullingProvider"/> which will be added</typeparam>
        /// <returns>The <see cref="ICullingProvider"/></returns>
        public T AddCulling<T>(T culling) where T : ICullingProvider
        {
            if (clientSideAdminToy.CullingProvider != null)
            {
                clientSideAdminToy.RemoveCulling();
            }

            clientSideAdminToy.CullingProvider = culling;
            SchematicSync.CullingProviders.Add(culling);
            return culling;
        }

        /// <summary>
        /// Removes <see cref="ICullingProvider"/> from the toy.
        /// </summary>
        public void RemoveCulling()
        {
            if (clientSideAdminToy.CullingProvider == null)
            {
                return;
            }

            SchematicSync.CullingProviders.Remove(clientSideAdminToy.CullingProvider);
            clientSideAdminToy.CullingProvider.Ignored.Clear();
            clientSideAdminToy.CullingProvider.Spawned.Clear();
            clientSideAdminToy.CullingProvider = null;
        }
    }
}
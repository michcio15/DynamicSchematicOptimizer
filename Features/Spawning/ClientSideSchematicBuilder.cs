using AdminToys;

using DynamicSchematicOptimizer.Features.Toys;

using Mirror;

using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable.Schematics;

using UnityEngine;

namespace DynamicSchematicOptimizer.Features.Spawning;

internal static class ClientSideSchematicBuilder
{
    internal static List<ClientSideAdminToy> Build(SchematicSpawnPlan plan, SchematicObject schematicObject,
        SchematicOptimisationConfig optimisationConfig)
    {
        uint rootNetId = schematicObject.GetComponent<NetworkIdentity>().netId;
        Dictionary<int, uint> netIdByObjectId = new(plan.ClientSideBlocks.Count);
        List<ClientSideAdminToy> toys = new(plan.ClientSideBlocks.Count);

        foreach (PlannedClientBlock plannedBlock in plan.ClientSideBlocks)
        {
            SchematicBlockData block = plannedBlock.Block;

            ClientSideAdminToy toy = CreateToy(block, optimisationConfig);
            toy.Position = block.Position;
            toy.Scale = block.Scale;
            toy.Rotation = Quaternion.Euler(block.Rotation);
            toy.MovementSmoothing = optimisationConfig.MovementSmoothing;
            toy.ParentNetId = ResolveParentNetId(plannedBlock, schematicObject, netIdByObjectId, rootNetId);

            netIdByObjectId[block.ObjectId] = toy.NetId;
            toys.Add(toy);
        }

        return toys;
    }

    private static uint ResolveParentNetId(PlannedClientBlock plannedBlock, SchematicObject schematicObject,
        Dictionary<int, uint> netIdByObjectId, uint rootNetId)
    {
        int parentId = plannedBlock.Block.ParentId;

        if (plannedBlock.ParentIsClientSide)
        {
            return netIdByObjectId[parentId];
        }

        if (!schematicObject.ObjectFromId.TryGetValue(parentId, out Transform parent))
        {
            Log.Warn($"Block {plannedBlock.Block.ObjectId} of {schematicObject.Name} has an unknown parent ID: {parentId} - parenting root");
            return rootNetId;
        }

        if (!parent.TryGetComponent(out NetworkIdentity networkIdentity))
        {
            Log.Warn($"{parent.name} does not have a {nameof(NetworkIdentity)} - {plannedBlock.Block.ObjectId} is parented to root");
            return rootNetId;
        }

        return networkIdentity.netId;
    }

    private static ClientSideAdminToy CreateToy(SchematicBlockData block, SchematicOptimisationConfig optimisationConfig)
    {
        return block.BlockType switch
        {
            BlockType.Primitive => SetupClientSidePrimitive(block, optimisationConfig.Primitive),
            BlockType.Text => SetupClientSideText(block, optimisationConfig.TextToy),
            BlockType.Light => SetupClientSideLight(block, optimisationConfig.LightSource),
            _ => throw new ArgumentOutOfRangeException($"Unsupported block type: {block.BlockType}"),
        };
    }

    private static ClientSidePrimitive SetupClientSidePrimitive(SchematicBlockData block, AdminToyOptimisationConfig optimisationConfigPrimitive)
    {
        ClientSidePrimitive clientSidePrimitive = new();
        Dictionary<string, object> properties = block.Properties;

        clientSidePrimitive.Type = (PrimitiveType)Convert.ToInt32(properties["PrimitiveType"]);
        clientSidePrimitive.Color = properties["Color"].ToString().GetColorFromString();

        PrimitiveFlags primitiveFlags;
        if (properties.TryGetValue("PrimitiveFlags", out object flags))
        {
            primitiveFlags = (PrimitiveFlags)Convert.ToByte(flags);
        }
        else
        {
            primitiveFlags = PrimitiveFlags.Visible;
            if (block.Scale.x >= 0f)
            {
                primitiveFlags |= PrimitiveFlags.Collidable;
            }
        }

        clientSidePrimitive.Flags = primitiveFlags;
        clientSidePrimitive.IsStatic = optimisationConfigPrimitive.IsStatic;

        return clientSidePrimitive;
    }

    private static ClientSideTextToy SetupClientSideText(SchematicBlockData block, AdminToyOptimisationConfig optimisationConfigTextToy)
    {
        ClientSideTextToy text = new();
        Dictionary<string, object> properties = block.Properties;

        text.TextFormat = Convert.ToString(properties["Text"]);
        text.Size = properties["DisplaySize"].ToVector2() * 20f;
        text.IsStatic = optimisationConfigTextToy.IsStatic;
        return text;
    }

    private static ClientSideAdminToy SetupClientSideLight(SchematicBlockData block, AdminToyOptimisationConfig optimisationConfigLightSource)
    {
        ClientSideLightSourceToy light = new();

        Dictionary<string, object> properties = block.Properties;

        light.LightType = properties.TryGetValue("LightType", out object lightType) ? (LightType)Convert.ToInt32(lightType) : LightType.Point;
        light.LightColor = properties["Color"].ToString().GetColorFromString();
        light.LightIntensity = Convert.ToSingle(properties["Intensity"]);
        light.LightRange = Convert.ToSingle(properties["Range"]);

        if (properties.TryGetValue("Shadows", out object shadows))
        {
            light.ShadowType = Convert.ToBoolean(shadows) ? LightShadows.Soft : LightShadows.None;
        }
        else
        {
            light.ShadowType = (LightShadows)Convert.ToInt32(properties["ShadowType"]);
#pragma warning disable CS0618 // Type or member is obsolete
            light.LightShape = (LightShape)Convert.ToInt32(properties["Shape"]);
#pragma warning restore CS0618 // Type or member is obsolete
            light.SpotAngle = Convert.ToSingle(properties["SpotAngle"]);
            light.InnerSpotAngle = Convert.ToSingle(properties["InnerSpotAngle"]);
            light.ShadowStrength = Convert.ToSingle(properties["ShadowStrength"]);
        }

        light.IsStatic = optimisationConfigLightSource.IsStatic;
        light.MovementSmoothing = 60;
        return light;
    }
}
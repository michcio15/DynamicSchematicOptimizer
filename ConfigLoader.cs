using System.Diagnostics.CodeAnalysis;

using DynamicSchematicOptimizer.Features.Spawning;

using LabApi.Loader.Features.Paths;
using LabApi.Loader.Features.Yaml;

using YamlDotNet.Serialization;

namespace DynamicSchematicOptimizer;

public static class ConfigLoader
{
    private const string OptimizerSubdirectory = "DynamicSchematicOptimizer";

    private static readonly Dictionary<string, SchematicOptimisationConfig> ByName = new();

    public static IDeserializer Deserializer => YamlConfigParser.Deserializer;
    public static ISerializer Serializer => YamlConfigParser.Serializer;

    public static void ReloadAll()
    {
        Init();
    }

    public static bool TryGetConfig(string name, [NotNullWhen(true)] out SchematicOptimisationConfig? config)
    {
        return ByName.TryGetValue(name, out config);
    }

    public static void CreateConfig(string name)
    {
        string schematicDir = Path.Combine(PathManager.Configs.FullName, OptimizerSubdirectory);

        Directory.CreateDirectory(schematicDir);

        string filePath = Path.Combine(schematicDir, $"{name}.yml");


        if (File.Exists(filePath))
        {
            return;
        }

        SchematicOptimisationConfig config = DynamicSchematicOptimizerPlugin.Config.DefaultConfig;

        string yaml = Serializer.Serialize(config);
        File.WriteAllText(filePath, yaml);

        ByName[name] = config;
    }

    public static void Init()
    {
        Disable();
        LoadAllConfigs();
    }

    public static void Disable()
    {
        ByName.Clear();
        ClientSideSchematicPlanner.Clear();
    }

    private static void LoadAllConfigs()
    {
        string schematicDir = Path.Combine(PathManager.Configs.FullName, OptimizerSubdirectory);

        Directory.CreateDirectory(schematicDir);

        IEnumerable<string> files = Directory.EnumerateFiles(schematicDir, "*.*", SearchOption.TopDirectoryOnly)
            .Where(static f => f.EndsWith(".yml", StringComparison.OrdinalIgnoreCase)
                               || f.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase));

        foreach (string file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);

            try
            {
                string yaml = File.ReadAllText(file);
                SchematicOptimisationConfig optimisationConfig =
                    Deserializer.Deserialize<SchematicOptimisationConfig>(yaml) ?? new SchematicOptimisationConfig();

                // updating the YAML for the updates
                string updatedYaml = Serializer.Serialize(optimisationConfig);
                if (updatedYaml != yaml)
                {
                    File.WriteAllText(file, updatedYaml);
                }

                ByName[name] = optimisationConfig;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to load config '{name}': {e}");
            }
        }
    }
}
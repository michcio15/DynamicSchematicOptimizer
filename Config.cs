using System.ComponentModel;

namespace DynamicSchematicOptimizer;

public class Config
{
    public bool Debug { get; set; } = false;

    [Description("If true then schematics not optimized by MERO will be optimized by DSO")]
    public bool UseMEROCompatibility { get; set; } = false;

    [Description("Names of schematics that will not be optimized by DSO if they are not optimized by MERO")]
    public List<string> ExcludedMEROSchematics { get; set; } = new();

    [Description("Default config for schematics that are not optimized by MERO if the compatiblity is enabled, also will be the deafult for newly created schematics")]
    public SchematicOptimisationConfig DefaultConfig { get; set; } = new();

    public float CullingTickTimeInBetween { get; set; } = 1;

    [Description("If true, DSO checks GitHub for a newer release on startup and downloads the plugin dll. The update is applied on the next server restart")]
    public bool EnableAutoUpdater { get; set; } = true;

    public CommandPermissionsConfig Permissions { get; set; } = new();
}

public class CommandPermissionsConfig
{
    public string Optimizer { get; set; } = "dso.use";
    public string Reload { get; set; } = "dso.reload";
    public string Create { get; set; } = "dso.create";
    public string Info { get; set; } = "dso.info";
    public string Culling { get; set; } = "dso.culling";
}
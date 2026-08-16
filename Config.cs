namespace DynamicSchematicOptimizer;

public class Config
{
    public bool Debug { get; set; } = false;
    public float CullingTickTimeInBetween { get; set; } = 1;

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
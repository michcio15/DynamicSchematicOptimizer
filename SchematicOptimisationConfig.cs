namespace DynamicSchematicOptimizer;

public class SchematicOptimisationConfig
{
    public bool Enabled { get; set; } = true;
    public byte CullingDistance { get; set; } = 33;
    public byte MovementSmoothing { get; set; } = 60;

    public PrimitiveToyOptimisationConfig Primitive { get; set; } = new()
    {
        Enabled = true,
        IsStatic = false,
        DontOptimizeWithCollision = true,
    };

    public AdminToyOptimisationConfig LightSource { get; set; } = new()
    {
        Enabled = true,
        IsStatic = false,
    };

    public AdminToyOptimisationConfig TextToy { get; set; } = new()
    {
        Enabled = true,
        IsStatic = false,
    };
}

public class PrimitiveToyOptimisationConfig : AdminToyOptimisationConfig
{
    public bool DontOptimizeWithCollision { get; set; } = true;
}

public class AdminToyOptimisationConfig
{
    public bool Enabled { get; set; } = true;
    public bool IsStatic { get; set; } = false;
}
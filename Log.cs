using LabApi.Features.Console;

namespace DynamicSchematicOptimizer;

internal static class Log
{
    internal static void Debug(object message)
    {
        Logger.Debug(message, DynamicSchematicOptimizerPlugin.Config.Debug);
    }

    internal static void Info(object message)
    {
        Logger.Info(message);
    }

    internal static void Error(object message)
    {
        Logger.Error(message);
    }

    internal static void Warn(object message)
    {
        Logger.Warn(message);
    }
}
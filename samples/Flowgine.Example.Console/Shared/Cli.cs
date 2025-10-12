namespace Flowgine.Example.Console.Shared;

public static class Cli
{
    public static string? GetArg(string[] args, string key, string? def = null)
    {
        for (int i = 0; i < args.Length; i++)
            if (args[i] == key && i + 1 < args.Length) return args[i + 1];
        return def;
    }
}
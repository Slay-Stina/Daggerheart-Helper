namespace DaggerheartHelper.Tests.Srd.Ingestion.Tests;

internal static class TestRepoPaths
{
    private const string SentinelPath = "External/daggerheart-srd/.build/03_json/armor.json";

    public static string LocalSrdJsonDirectory => Path.Combine(AppContext.BaseDirectory, "Srd.Ingestion.Tests", "TestData", "Json");

    public static string SrdJsonDirectory =>
        TryFindRepositoryRoot(out var root)
            ? Path.Combine(root, "External", "daggerheart-srd", ".build", "03_json")
            : LocalSrdJsonDirectory;

    public static bool HasExternalSrdJson => TryFindRepositoryRoot(out _);

    private static bool TryFindRepositoryRoot(out string root)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, SentinelPath)))
            {
                root = current.FullName;
                return true;
            }

            current = current.Parent;
        }

        // In git worktree setups, SRD build artifacts may exist in a sibling worktree root.
        var siblingRoot = FindSiblingWorktreeRoot();
        if (siblingRoot is not null)
        {
            root = siblingRoot;
            return true;
        }

        root = string.Empty;
        return false;
    }

    private static string? FindSiblingWorktreeRoot()
    {
        var workspacesParent = new DirectoryInfo(AppContext.BaseDirectory);
        while (workspacesParent is not null && workspacesParent.Name != "RiderProjects")
        {
            workspacesParent = workspacesParent.Parent;
        }

        if (workspacesParent is null)
        {
            return null;
        }

        foreach (var directory in workspacesParent.EnumerateDirectories("Daggerheart-Helper*"))
        {
            if (File.Exists(Path.Combine(directory.FullName, SentinelPath)))
            {
                return directory.FullName;
            }
        }

        return null;
    }
}


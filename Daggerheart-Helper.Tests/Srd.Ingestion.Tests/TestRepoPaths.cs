namespace DaggerheartHelper.Tests.Srd.Ingestion.Tests;

internal static class TestRepoPaths
{
    private const string SentinelPath = "External/daggerheart-srd/.build/03_json/armor.json";

    public static string RepositoryRoot => FindRepositoryRoot();

    public static string SrdJsonDirectory => Path.Combine(RepositoryRoot, "External", "daggerheart-srd", ".build", "03_json");

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, SentinelPath)))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        // In git worktree setups, SRD build artifacts may exist in a sibling worktree root.
        var siblingRoot = FindSiblingWorktreeRoot();
        if (siblingRoot is not null)
        {
            return siblingRoot;
        }

        throw new DirectoryNotFoundException($"Could not locate repository root containing '{SentinelPath}'.");
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


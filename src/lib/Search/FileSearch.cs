using GlobExpressions;

namespace Makspll.Pathfinder.Search;


public static class FileSearch
{
    /*
     * Recursively yield all files under a directory
     */
    static IEnumerable<string> AllFilesUnder(string searchRootDir)
    {
        var searchRoot = new DirectoryInfo(searchRootDir);
        if (!searchRoot.Exists || !searchRoot.Attributes.HasFlag(FileAttributes.Directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {searchRootDir}");
        }

        foreach (var file in searchRoot.GetFiles())
        {
            yield return file.FullName;
        }

        foreach (var dir in searchRoot.GetDirectories())
        {
            foreach (var file in AllFilesUnder(dir.FullName))
            {
                yield return file;
            }
        }
    }

    /*
     * Finds all files matching the given glob patterns under the given directory recursively
     */
    public static IEnumerable<string> FindAllFiles(IEnumerable<string> fileGlobs, string searchRootDir)
    {
        var globs = fileGlobs.Select(g => new Glob(g)).ToList();
        var searchRoot = new DirectoryInfo(searchRootDir);
        foreach (var file in AllFilesUnder(searchRootDir))
        {
            if (globs.Any(g => g.IsMatch(file)))
            {
                yield return file;
            }
        }
    }

    /// <summary>
    /// Finds the nearest file with the given name in the directory tree starting from the given directory.
    /// </summary>
    public static FileInfo? FindNearestFile(string filename, string searchRootDir, int limit = 20)
    {
        var searchRoot = new DirectoryInfo(searchRootDir);
        if (!searchRoot.Exists || !searchRoot.Attributes.HasFlag(FileAttributes.Directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {searchRootDir}");
        }

        var currentDir = searchRoot;
        while (currentDir != null && limit-- > 0)
        {
            var file = new FileInfo(Path.Combine(currentDir.FullName, filename));
            if (file.Exists)
            {
                return file;
            }

            currentDir = currentDir.Parent;
        }

        return null;
    }
}

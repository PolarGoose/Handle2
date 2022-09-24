namespace Handle2.HandleInfo.Utils;

public static class FileUtils
{
    public static string AddTrailingSeparatorIfItIsAFolder(string fileOrFolderPath)
    {
        return Directory.Exists(fileOrFolderPath) ? fileOrFolderPath + '\\' : fileOrFolderPath;
    }

    public static string ToCanonicalPath(string fileOrFolderPath)
    {
        var p = Path.GetFullPath(fileOrFolderPath);
        if (!p.EndsWith('\\') && Directory.Exists(p))
        {
            p += '\\';
        }

        return p;
    }

    public static bool Exists(string path)
    {
        return File.Exists(path) || Directory.Exists(path);
    }
}

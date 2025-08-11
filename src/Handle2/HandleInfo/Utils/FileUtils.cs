namespace Handle2.HandleInfo.Utils;

public static class FileUtils
{
    public static string AddTrailingSeparatorIfItIsAFolder(string fileOrFolderPath)
    {
        if (fileOrFolderPath.EndsWith('\\'))
        {
            return fileOrFolderPath;
        }

        return Directory.Exists(fileOrFolderPath) ? fileOrFolderPath + '\\' : fileOrFolderPath;
    }

    public static string ToCanonicalPath(string fileOrFolderPath)
    {
        return AddTrailingSeparatorIfItIsAFolder(Path.GetFullPath(fileOrFolderPath));
    }

    public static bool Exists(string path)
    {
        return File.Exists(path) || Directory.Exists(path);
    }
}

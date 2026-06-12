namespace Handle2.HandleInfo.Utils;

internal static class MappedDriveResolver
{
    [System.Runtime.InteropServices.DllImport("mpr.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    private static extern int WNetGetConnection(string lpLocalName, System.Text.StringBuilder lpRemoteName, ref int lpnLength);

    private const int NO_ERROR = 0;
    private const int ERROR_MORE_DATA = 234;

    public static string ResolveToUncPathIfNetworkDrivePath(string path)
    {
        var root = Path.GetPathRoot(path);
        var drive = root!.Substring(0, 2);

        var uncRoot = GetUncRootForDrive(drive);
        if (uncRoot is null)
        {
            return path;
        }

        var relativePart = path.Substring(2);
        return uncRoot + relativePart;
    }

    private static string? GetUncRootForDrive(string drive)
    {
        var remoteName = new System.Text.StringBuilder(1000);
        var capacity = remoteName.Capacity;

        var result = WNetGetConnection(drive, remoteName, ref capacity);
        if (result == NO_ERROR)
        {
            return remoteName.ToString();
        }

        if (result == ERROR_MORE_DATA)
        {
            remoteName = new System.Text.StringBuilder(capacity);
            result = WNetGetConnection(drive, remoteName, ref capacity);
            if (result == NO_ERROR)
            {
                return remoteName.ToString();
            }
        }

        return null;
    }
}

internal sealed class CanonicalPath
{
    public string Path { get; private set; }
    public bool IsDirectory { get; }

    public CanonicalPath(string path)
    {
        Path = System.IO.Path.GetFullPath(path).Replace('/', '\\');
        if (!Path.StartsWith(@"\\"))
        {
            Path = MappedDriveResolver.ResolveToUncPathIfNetworkDrivePath(Path);
        }

        IsDirectory = Directory.Exists(Path);
        if (IsDirectory && !Path.EndsWith("\\"))
        {
            Path += "\\";
        }
    }
}

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
        return new CanonicalPath(fileOrFolderPath).Path;
    }

    public static bool Exists(string path)
    {
        return File.Exists(path) || Directory.Exists(path);
    }
}

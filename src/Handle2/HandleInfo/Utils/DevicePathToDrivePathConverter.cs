using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Text;

namespace Handle2.HandleInfo.Utils;

// Creates a conversion map {DeviceName, MountPath} from two sources:
//
// 1. DOS devices (QueryDosDevice): maps drive letters to their device names.
// Example:
//   [ {"\Device\HardDiskVolume2\"                           , "C:\"},
//     {"\Device\HardDiskVolume15\"                          , "D:\"},
//     {"\Device\VBoxMiniRdr\;H:\VBoxSvr\My-H\"              , "H:\"},
//     {"\Device\LanmanRedirector\;I:000215d7\10.22.3.84\i\" , "I:\"},
//     {"\Device\CdRom0\"                                    , "X:\"} ]
//
// 2. Mounted volumes (FindFirstVolume/FindNextVolume): maps volume GUID paths to mount points.
// Example:
//   [ {"\Device\Volume{470e4501-335c-11f1-b2d8-2c0da7fb0517}\", "E:\"},
//     {"\Device\Volume{a1b2c3d4-5678-90ab-cdef-1234567890ab}\", "C:\Mount\"} ]
internal sealed class DevicePathToDrivePathConverter
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "QueryDosDeviceW")]
    private static extern uint QueryDosDeviceW(string lpDeviceName, [Out] StringBuilder lpTargetPath, uint ucchMax);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern SafeFindVolumeHandle FindFirstVolumeW([Out] StringBuilder lpszVolumeName, int cchBufferLength);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FindNextVolumeW(SafeFindVolumeHandle hFindVolume, [Out] StringBuilder lpszVolumeName, int cchBufferLength);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FindVolumeClose(IntPtr hFindVolume);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetVolumePathNamesForVolumeNameW(string lpszVolumeName, [Out] StringBuilder? lpszVolumePathNames, uint cchBufferLength, out uint lpcchReturnLength);

    private sealed class SafeFindVolumeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeFindVolumeHandle()
            : base(true)
        {
        }

        public SafeFindVolumeHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return FindVolumeClose(handle);
        }
    }

    private readonly Dictionary<string, string> deviceNameToMountPathMap = new();

    public DevicePathToDrivePathConverter()
    {
        PopulateDosDevices();
        PopulateMountedVolumes();
    }

    public string? GetDriveLetterBasedFullName(string deviceBasedFileFullName)
    {
        foreach (var pair in deviceNameToMountPathMap)
        {
            if (deviceBasedFileFullName.StartsWith(pair.Key, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value + deviceBasedFileFullName.Substring(pair.Key.Length);
            }
        }

        return null;
    }

    private void PopulateDosDevices()
    {
        var deviceNameBuf = new StringBuilder(1000);
        for (var driveLetter = 'A'; driveLetter <= 'Z'; driveLetter++)
        {
            var drive = driveLetter + ":";

            var length = QueryDosDeviceW(drive, deviceNameBuf, (uint)deviceNameBuf.Capacity);
            if (length == 0)
            {
                continue;
            }

            var deviceName = deviceNameBuf.ToString();
            if (string.IsNullOrEmpty(deviceName))
            {
                continue;
            }

            deviceNameToMountPathMap[deviceName + "\\"] = drive + "\\";
            deviceNameBuf.Clear();
        }
    }

    private void PopulateMountedVolumes()
    {
        var volumeNameBuf = new StringBuilder(1000);

        using var findHandle = FindFirstVolumeW(volumeNameBuf, volumeNameBuf.Capacity);
        if (findHandle.IsInvalid)
        {
            return;
        }

        do
        {
            var volumeName = volumeNameBuf.ToString();
            var mountPath = GetMountPathForVolume(volumeName);
            if (mountPath is not string nonNullMountPath || nonNullMountPath.Length == 0)
            {
                volumeNameBuf.Clear();
                continue;
            }

            deviceNameToMountPathMap[@"\Device\" + volumeName.Substring(4)] = nonNullMountPath;
            volumeNameBuf.Clear();
        }
        while (FindNextVolumeW(findHandle, volumeNameBuf, volumeNameBuf.Capacity));
    }

    private static string? GetMountPathForVolume(string volumeName)
    {
        GetVolumePathNamesForVolumeNameW(volumeName, null, 0, out var charCount);
        if (charCount == 0)
        {
            return null;
        }

        var pathNamesBuf = new StringBuilder((int)charCount);
        if (!GetVolumePathNamesForVolumeNameW(volumeName, pathNamesBuf, charCount, out _))
        {
            return null;
        }

        return pathNamesBuf.ToString();
    }
}

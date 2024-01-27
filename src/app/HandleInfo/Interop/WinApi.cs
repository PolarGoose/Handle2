using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Text;

namespace App.HandleInfo.Interop;

public class WinApi
{
    [Flags]
    public enum StandardAccessRights : long
    {
        DELETE = 0x00010000L,
        READ_CONTROL = 0x00020000L,
        WRITE_DAC = 0x00040000L,
        WRITE_OWNER = 0x00080000L,
        SYNCHRONIZE = 0x00100000L,
        STANDARD_RIGHTS_REQUIRED = 0x000F0000L
    }

    [Flags]
    public enum ProcessAccessRights : long
    {
        PROCESS_ALL_ACCESS = StandardAccessRights.STANDARD_RIGHTS_REQUIRED | StandardAccessRights.SYNCHRONIZE | 0xFFFF,
        PROCESS_TERMINATE = 0x0001,
        PROCESS_CREATE_THREAD = 0x0002,
        PROCESS_SET_SESSIONID = 0x0004,
        PROCESS_VM_OPERATION = 0x0008,
        PROCESS_VM_READ = 0x0010,
        PROCESS_VM_WRITE = 0x0020,
        PROCESS_DUP_HANDLE = 0x0040,
        PROCESS_CREATE_PROCESS = 0x0080,
        PROCESS_SET_QUOTA = 0x0100,
        PROCESS_SET_INFORMATION = 0x0200,
        PROCESS_QUERY_INFORMATION = 0x0400,
        PROCESS_SUSPEND_RESUME = 0x0800,
        PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
        PROCESS_SET_LIMITED_INFORMATION = 0x2000
    }

    [Flags]
    public enum DuplicateObjectOptions : uint
    {
        DUPLICATE_CLOSE_SOURCE = 0x00000001,
        DUPLICATE_SAME_ACCESS = 0x00000002,
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern SafeProcessHandle OpenProcess(
        ProcessAccessRights dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        UIntPtr dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DuplicateHandle(
        SafeProcessHandle hSourceProcessHandle,
        IntPtr hSourceHandle,
        SafeProcessHandle hTargetProcessHandle,
        out SafeFileHandle lpTargetHandle,
        uint dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        uint dwOptions);

    [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "GetCurrentProcess")]
    private static extern IntPtr GetCurrentProcessPrivate();
    public static SafeProcessHandle GetCurrentProcess()
    {
        IntPtr handle = GetCurrentProcessPrivate();
        return new SafeProcessHandle(handle, ownsHandle: false);
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int GetFinalPathNameByHandleW(SafeFileHandle hFile, [Out] StringBuilder filePathBuffer, int filePathBufferSize, int flags);

    public static string? GetFinalPathNameByHandle(SafeFileHandle hFile)
    {
        var buf = new StringBuilder();
        var result = GetFinalPathNameByHandleW(hFile, buf, buf.Capacity, 0);
        if(result == 0)
        {
            return null;
        }

        buf.EnsureCapacity(result);
        result = GetFinalPathNameByHandleW(hFile, buf, buf.Capacity, 0);
        if (result == 0)
        {
            return null;
        }

        var str = buf.ToString();
        return str.StartsWith(@"\\?\") ? str.Substring(4) : str;
    }
}

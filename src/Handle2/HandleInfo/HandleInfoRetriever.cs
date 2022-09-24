using Handle2.HandleInfo.Interop;
using Handle2.HandleInfo.Utils;
using Microsoft.Win32.SafeHandles;

namespace Handle2.HandleInfo;

public record struct HandleInfo(
    // The result of NtDll.NtQueryObject(OBJECT_INFORMATION_CLASS.ObjectTypeInformation).
    // Examples:
    // * File
    // * Event
    string? HandleType,

    // the result ot WinApi.GetFileType().
    WinApi.FileType? FileType,

    // The result of  NtDll.NtQueryObject(OBJECT_INFORMATION_CLASS.ObjectNameInformation).
    // Examples:
    // * \\REGISTRY\\MACHINE\\SYSTEM\\ControlSet001\\Control\\Session Manager
    // * \\Device\\HarddiskVolume3\\Windows\\System32
    string? Name,

    // If the handle is a file or folder this field contains its full path.
    // Examples:
    // * "C:\Users\user\file.txt"
    // * "C:\Users\user\"
    string? FullNameIfItIsAFileOrAFolder,

    // The content of the SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX.GrantedAccess field.
    uint GrantedAccess,

    // The content of the SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX.HandleAttributes field.
    uint Attributes,

    // The content of the SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX.Object field.
    ulong AddressInTheKernelMemory);

public record struct ProcessInfo(
    ulong Pid,
    string? ProcessExecutablePath,
    string? UserName,
    string? DomainName,
    List<HandleInfo> Handles);

public static class HandleInfoRetriever
{
    private static IEnumerable<IGrouping<UIntPtr, NtDll.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX>> QuerySystemHandleInformation()
    {
        return NtDll.QuerySystemHandleInformation().GroupBy(handle => handle.UniqueProcessId);
    }

    private static void AddHandleTypeAndNameInfo(SafeFileHandle handle, ref HandleInfo handleInfo)
    {
        handleInfo.FileType = WinApi.GetFileType(handle);
        handleInfo.Name = NtDll.GetHandleName(handle);
        if (handleInfo.FileType == WinApi.FileType.FILE_TYPE_DISK)
        {
            handleInfo.FullNameIfItIsAFileOrAFolder = WinApi.GetFinalPathNameByHandle(handle);
            if (handleInfo.FullNameIfItIsAFileOrAFolder is not null)
            {
                handleInfo.FullNameIfItIsAFileOrAFolder = FileUtils.AddTrailingSeparatorIfItIsAFolder(handleInfo.FullNameIfItIsAFileOrAFolder);
            }
        }
    }

    public static IEnumerable<ProcessInfo> GetAllProcInfos()
    {
        var currentProcess = WinApi.GetCurrentProcess();
        var result = new List<ProcessInfo>();

        foreach (var (pid, handles) in QuerySystemHandleInformation())
        {
            using var openedProcess = ProcessUtils.OpenProcessToDuplicateHandle(pid);
            if (openedProcess is null)
            {
                continue;
            }

            var processInfo = new ProcessInfo
            {
                Pid = pid.ToUInt64(),
                Handles = []
            };

            foreach (var h in handles)
            {
                using var dupHandle = WinApi.DuplicateHandle(currentProcess, openedProcess, h);
                if (dupHandle.IsInvalid)
                {
                    continue;
                }

                var handle = new HandleInfo
                {
                    GrantedAccess = h.GrantedAccess,
                    Attributes = h.HandleAttributes,
                    AddressInTheKernelMemory = (ulong)h.Object.ToInt64(),
                    HandleType = NtDll.GetHandleType(dupHandle)
                };

                if (handle.HandleType is not null)
                {
                    var task = Task.Run(() =>
                    {
                        AddHandleTypeAndNameInfo(dupHandle, ref handle);
                    });
                    task.Wait(TimeSpan.FromMilliseconds(300));
                }

                processInfo.Handles.Add(handle);
            }

            if (processInfo.Handles.Any())
            {
                (processInfo.DomainName, processInfo.UserName) = ProcessUtils.GetOwnerDomainAndUserNames(openedProcess);
                processInfo.ProcessExecutablePath = ProcessUtils.GetExecutablePath(pid);
                result.Add(processInfo);
            }
        }

        return result;
    }

    public static IEnumerable<ProcessInfo> GetProcInfosLockingPath(string path)
    {
        var currentProcess = WinApi.GetCurrentProcess();
        var result = new List<ProcessInfo>();
        path = FileUtils.ToCanonicalPath(path);

        foreach (var (pid, handles) in QuerySystemHandleInformation())
        {
            using var openedProcess = ProcessUtils.OpenProcessToDuplicateHandle(pid);
            if (openedProcess is null)
            {
                continue;
            }

            var processInfo = new ProcessInfo
            {
                Pid = pid.ToUInt64(),
                Handles = []
            };

            foreach (var h in handles)
            {
                using var dupHandle = WinApi.DuplicateHandle(currentProcess, openedProcess, h);
                if (dupHandle.IsInvalid)
                {
                    continue;
                }

                using var reopenedHandle = WinApi.ReOpenFile(dupHandle, WinApi.FileDesiredAccess.None, FileShare.None, WinApi.FileFlagsAndAttributes.None);
                if (reopenedHandle.IsInvalid)
                {
                    continue;
                }

                var handle = new HandleInfo
                {
                    GrantedAccess = h.GrantedAccess,
                    Attributes = h.HandleAttributes,
                    AddressInTheKernelMemory = (ulong)h.Object.ToInt64(),
                    HandleType = NtDll.GetHandleType(reopenedHandle)
                };

                if (handle.HandleType is not null)
                {
                    AddHandleTypeAndNameInfo(reopenedHandle, ref handle);
                }

                if (handle.FullNameIfItIsAFileOrAFolder?.StartsWith(path, StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    processInfo.Handles.Add(handle);
                }
            }

            if (processInfo.Handles.Any())
            {
                (processInfo.DomainName, processInfo.UserName) = ProcessUtils.GetOwnerDomainAndUserNames(openedProcess);
                processInfo.ProcessExecutablePath = ProcessUtils.GetExecutablePath(pid);

                result.Add(processInfo);
            }
        }

        return result;
    }
}

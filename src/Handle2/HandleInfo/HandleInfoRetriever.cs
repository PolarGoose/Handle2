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
    IEnumerable<HandleInfo> Handles,
    IEnumerable<string> ModuleNames);

public static class HandleInfoRetriever
{
    private static IEnumerable<IGrouping<UIntPtr, NtDll.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX>> QuerySystemHandleInformation()
    {
        return NtDll.QuerySystemHandleInformation().GroupBy(handle => handle.UniqueProcessId);
    }

    private static void AddHandleTypeAndNameInfo(SafeFileHandle handle, DevicePathToDrivePathConverter devicePathToDrivePathConverter, ref HandleInfo handleInfo)
    {
        handleInfo.FileType = WinApi.GetFileType(handle);
        handleInfo.Name = NtDll.GetHandleName(handle);
        if (handleInfo.HandleType == "File" && handleInfo.Name is not null)
        {
            handleInfo.FullNameIfItIsAFileOrAFolder = ToWindowsPath(devicePathToDrivePathConverter, handleInfo.Name);
        }
    }

    private static IEnumerable<ProcessInfo> GetProcInfos(Func<HandleInfo, bool> handleFilter, Func<string, bool> moduleNameFilter, bool onlyFileHandles)
    {
        using var currentProcess = WinApi.GetCurrentProcess();
        var devicePathToDrivePathConverter = new DevicePathToDrivePathConverter();
        var result = new List<ProcessInfo>();

        var processes = QuerySystemHandleInformation().Select(processAndHandles => (processAndHandles.Key, processAndHandles.ToArray())).ToArray();
        var currentProcessIndex = 0;
        var currentHandleIndex = 0;
        SafeProcessHandle? currentOpenedProcess = null;
        var currentHandles = new List<HandleInfo>();
        SafeFileHandle? currentDupHandle = null;
        ushort? fileHandleObjectTypeIndex = null;

        while (currentProcessIndex < processes.Length)
        {
            WorkerThreadWithDeadLockDetection.Run(TimeSpan.FromMilliseconds(50), watchdog =>
            {
                while (currentProcessIndex < processes.Length)
                {
                    var (pid, handles) = processes[currentProcessIndex];

                    if (currentOpenedProcess is null)
                    {
                        currentOpenedProcess = ProcessUtils.OpenProcessToDuplicateHandle(pid);
                        if (currentOpenedProcess is null)
                        {
                            currentProcessIndex++;
                            continue;
                        }

                        currentHandles = new List<HandleInfo>();
                        currentHandleIndex = 0;
                    }

                    while (currentHandleIndex < handles.Length)
                    {
                        currentDupHandle?.Dispose();
                        var h = handles[currentHandleIndex];
                        currentHandleIndex++;

                        if (onlyFileHandles && fileHandleObjectTypeIndex is not null && h.ObjectTypeIndex != fileHandleObjectTypeIndex)
                        {
                            continue;
                        }

                        currentDupHandle = WinApi.DuplicateHandle(currentProcess, currentOpenedProcess, h);
                        if (currentDupHandle.IsInvalid)
                        {
                            continue;
                        }

                        var handle = new HandleInfo
                        {
                            GrantedAccess = h.GrantedAccess,
                            Attributes = h.HandleAttributes,
                            AddressInTheKernelMemory = (ulong)h.Object.ToInt64(),
                        };

                        watchdog.Arm();
                        try
                        {
                            handle.HandleType = NtDll.GetHandleType(currentDupHandle);
                            if (onlyFileHandles && handle.HandleType != "File")
                            {
                                continue;
                            }

                            if (handle.HandleType == "File")
                            {
                                fileHandleObjectTypeIndex = h.ObjectTypeIndex;
                            }

                            if (handle.HandleType is not null)
                            {
                                AddHandleTypeAndNameInfo(currentDupHandle, devicePathToDrivePathConverter, ref handle);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        finally
                        {
                            watchdog.Disarm();
                        }

                        if(handleFilter(handle))
                        {
                            currentHandles.Add(handle);
                        }
                    }

                    var moduleNames = ProcessUtils.GetProcessModules(currentOpenedProcess)
                                                  .Select(name => ToWindowsPath(devicePathToDrivePathConverter, name) ?? name)
                                                  .Where(moduleNameFilter)
                                                  .ToArray();

                    if (currentHandles.Any() || moduleNames.Any())
                    {
                        var procInfo = new ProcessInfo
                        {
                            Pid = pid.ToUInt64(),
                            Handles = currentHandles,
                            ModuleNames = moduleNames
                        };
                        (procInfo.DomainName, procInfo.UserName) = ProcessUtils.GetOwnerDomainAndUserNames(currentOpenedProcess);
                        procInfo.ProcessExecutablePath = ProcessUtils.GetProcessExeFullName(currentOpenedProcess);
                        result.Add(procInfo);
                    }

                    currentDupHandle?.Dispose();
                    currentOpenedProcess.Dispose();
                    currentOpenedProcess = null;
                    currentProcessIndex++;
                }
            });
        }

        return result;
    }

    public static IEnumerable<ProcessInfo> GetAllProcInfos()
    {
        return GetProcInfos(_ => true, _ => true, false);
    }

    public static IEnumerable<ProcessInfo> GetProcInfosLockingPath(string path)
    {
        var canonicalPath = new CanonicalPath(path);
        if (canonicalPath.IsDirectory)
        {
            return GetProcInfos(
                handle => handle.FullNameIfItIsAFileOrAFolder?.StartsWith(canonicalPath.Path, StringComparison.InvariantCultureIgnoreCase) == true,
                moduleName => moduleName.StartsWith(canonicalPath.Path, StringComparison.InvariantCultureIgnoreCase),
                true);
        }

        return GetProcInfos(
            handle => string.Equals(handle.FullNameIfItIsAFileOrAFolder, canonicalPath.Path, StringComparison.InvariantCultureIgnoreCase),
            moduleName => string.Equals(moduleName, canonicalPath.Path, StringComparison.InvariantCultureIgnoreCase),
            true);
    }

    private static string? ToWindowsPath(DevicePathToDrivePathConverter devicePathToDrivePathConverter, string devicePath)
    {
        if (devicePath.StartsWith("\\Device\\Mup\\"))
        {
            return FileUtils.AddTrailingSeparatorIfItIsAFolder("\\" + devicePath.Substring(11));
        }

        if (devicePath.StartsWith("\\Device\\") && devicePathToDrivePathConverter.GetDriveLetterBasedFullName(devicePath) is { } path)
        {
            return FileUtils.AddTrailingSeparatorIfItIsAFolder(path);
        }

        return null;
    }
}
